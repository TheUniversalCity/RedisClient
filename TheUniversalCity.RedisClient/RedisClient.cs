using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TheUniversalCity.RedisClient.Exceptions;
using TheUniversalCity.RedisClient.InMemory;
using TheUniversalCity.RedisClient.RedisObjects;
using TheUniversalCity.RedisClient.RedisObjects.Agregates;
using TheUniversalCity.RedisClient.RedisObjects.BlobStrings;
using TheUniversalCity.RedisClient.RedisObjects.Numerics;
using TheUniversalCity.RedisClient.RedisObjects.SimpleStrings;
using TheUniversalCity.RedisClient.Streaming;

namespace TheUniversalCity.RedisClient
{
    public sealed class RedisClient : IDisposable
    {
        private readonly BlockingCollection<TaskCompletionSource<RedisObject>> redisCompletedTasks = new BlockingCollection<TaskCompletionSource<RedisObject>>();
        private readonly BlockingCollection<TaskCompletionSource<RedisObject>> emergentRedisCompletedTasks = new BlockingCollection<TaskCompletionSource<RedisObject>>();

        public event Action<Exception, Socket> OnException;
        public event Action<Exception, Socket> OnConnectionFailed;
        public event Action<RedisPushType> OnPushMessageReceived;

        private bool disposedValue;

        public RedisConfiguration Configuration;

        public DnsEndPointTCPConnector.Enumerator ReceiverEnumerator { get; set; }
        private readonly Task ReceiveWorkerTask;

        private readonly RedisClientInMemoryDictionary keyValuePairs = new RedisClientInMemoryDictionary();

        private readonly Func<Type, string, Func<Task<object>>, CancellationToken, TimeSpan?, Task<object>> _GetOrUpdateAsync;
        private readonly Func<Type, string, CancellationToken, Task<object>> _GetAsync;
        private readonly Func<string, CancellationToken, Task<string>> _GetAsStringAsync;

        private readonly Func<Type, string, Func<object>, CancellationToken, TimeSpan?, object> _GetOrUpdate;
        private readonly Func<Type, string, CancellationToken, object> _Get;
        private readonly Func<string, CancellationToken, string> _GetAsString;

        private volatile int recentlyConnected = 2;
        private readonly AutoResetEvent emergencyStartAutoEvent = new AutoResetEvent(false);
        public bool IsConnected { get { return ReceiverEnumerator.Socket?.Connected ?? false; } }

        public DnsEndPointTCPConnector Receiver { get; }

        private RedisClient(RedisConfiguration configuration)
        {
            this.Configuration = configuration;

            if (configuration.DnsEndPoints.Count == 0)
            {
                throw new InvalidOperationException("No endpoint configuration");
            }

            Receiver = new DnsEndPointTCPConnector(
                configuration.DnsEndPoints.ToArray(),
                Configuration.Logger,
                configuration.ReceiveBufferSize,
                configuration.SendBufferSize,
                configuration.ConnectRetry,
                configuration.ConnectRetryInterval);

            Receiver.OnException += ReceiverEnumerator_OnException;
            Receiver.OnConnectionTryFailed += ReceiverEnumerator_OnConnectionTryFailed;
            Receiver.OnConnected += ReceiverEnumerator_OnConnected;
            Receiver.OnReceiverDisconnect += ReceiverEnumerator_OnReceiverDisconnect;
            Receiver.OnSenderDisconnect += Receiver_OnSenderDisconnect;

            ReceiverEnumerator = Receiver.GetEnumerator() as DnsEndPointTCPConnector.Enumerator; // Create new socket connection

            ReceiveWorkerTask = Task.Factory.StartNew(ReceiveWorker, this, TaskCreationOptions.LongRunning);
            Task.Factory.StartNew(StarterWorker, this, TaskCreationOptions.LongRunning);

            if (configuration.ClientCache)
            {
                _GetOrUpdateAsync = new Func<Type, string, Func<Task<object>>, CancellationToken, TimeSpan?, Task<object>>(GetOrUpdateWithLocalCacheAsync);
                _GetAsync = new Func<Type, string, CancellationToken, Task<object>>(GetWithLocalCacheAsync);
                _GetAsStringAsync = new Func<string, CancellationToken, Task<string>>(GetWithLocalCacheAsync);

                _GetOrUpdate = new Func<Type, string, Func<object>, CancellationToken, TimeSpan?, object>(GetOrUpdateWithLocalCache);
                _Get = new Func<Type, string, CancellationToken, object>(GetWithLocalCache);
                _GetAsString = new Func<string, CancellationToken, string>(GetWithLocalCache);
            }
            else
            {
                _GetOrUpdateAsync = new Func<Type, string, Func<Task<object>>, CancellationToken, TimeSpan?, Task<object>>(GetOrUpdateWithoutLocalCacheAsync);
                _GetAsync = new Func<Type, string, CancellationToken, Task<object>>(GetWithoutLocalCacheAsync);
                _GetAsStringAsync = new Func<string, CancellationToken, Task<string>>(GetWithoutLocalCacheAsync);

                _GetOrUpdate = new Func<Type, string, Func<object>, CancellationToken, TimeSpan?, object>(GetOrUpdateWithoutLocalCache);
                _Get = new Func<Type, string, CancellationToken, object>(GetWithoutLocalCache);
                _GetAsString = new Func<string, CancellationToken, string>(GetWithoutLocalCache);
            }
        }

        private void ReceiverEnumerator_OnConnected(DnsEndPointTCPConnector.Enumerator arg1)
        {
#if DEBUG
            Configuration.Logger(nameof(ReceiverEnumerator_OnConnected));
#endif
            Interlocked.CompareExchange(ref recentlyConnected, 1, 0);
            emergencyStartAutoEvent.Set();
        }

        public async Task StartAsync()
        {
            RedisObject obj1, obj2, obj3, obj4;

            ReceiverEnumerator.BeginEmergency();

            try
            {
                while (true)
                {
                    if (Configuration.Options.ContainsKey(RedisConfiguration.PASSWORD_KEY))
                    {
                        // Auth isteği patlayabilir yeniden denememesi lazım.
                        obj1 = await AuthAsync();
                    }

                    if (!await CheckMasterAsync())
                    {
                        ReceiverEnumerator.Reset(true, true);

                        continue;
                    }

                    if (Configuration.DB > 0)
                    {
                        obj2 = await SelectDbAsync(Configuration.DB);
                    }

                    if (Configuration.ClientCache)
                    {
                        obj3 = await Hello3Async();
                        obj4 = await ClientTrackingOnAsync();
                    }

                    break;
                }
            }
            catch (RedisClientNotConectedException ex)
            {
                Configuration.Logger("Start Error : " + ex.Message);
            }
            finally
            {
                Interlocked.CompareExchange(ref recentlyConnected, 0, 2);
                ReceiverEnumerator.EndEmergency();
                Configuration.Logger("Start Emergency Disposing");
            }

            //startComplete.Set();
        }

        public void BeginEmergency()
        {
            ReceiverEnumerator.BeginEmergency();
        }

        public void EndEmergency()
        {
            ReceiverEnumerator.EndEmergency();
        }

        private void Receiver_OnSenderDisconnect(Exception arg1, DnsEndPointTCPConnector.Enumerator arg2)
        {

        }

        private void ReceiverEnumerator_OnReceiverDisconnect(Exception arg1, DnsEndPointTCPConnector.Enumerator arg2)
        {
            var emergentCount = emergentRedisCompletedTasks.Count;

            for (int i = 0; i < emergentCount; i++)
            {
                emergentRedisCompletedTasks.Take().SetException(arg1);
            }

            ReceiverEnumerator.BeginEmergency();

            var count = redisCompletedTasks.Count - arg2.bufferedMessageCounter;
#if DEBUG
            Configuration.Logger($"{nameof(ReceiverEnumerator_OnReceiverDisconnect)} : Count => {count}, taskCompletionSourcesCount => {redisCompletedTasks.Count}, arg2.bufferedMessageCounter => {arg2.bufferedMessageCounter}");
#endif
            for (int i = 0; i < count; i++)
            {
                redisCompletedTasks.Take().SetException(arg1);
            }

            keyValuePairs.Clear();
        }

        private void ReceiverEnumerator_OnConnectionTryFailed(DnsEndPoint arg1, DnsEndPointTCPConnector.Enumerator arg2)
        {
            OnConnectionFailed?.Invoke(new InvalidOperationException($"{arg1} Endpoint can not reach"), arg2.Socket);
        }

        private void ReceiverEnumerator_OnException(Exception arg1, DnsEndPointTCPConnector.Enumerator arg2)
        {
            OnException?.Invoke(arg1, arg2.Socket);
        }

        private void StarterWorker(object state)
        {
            while (true)
            {
                emergencyStartAutoEvent.WaitOne();
                try
                {
                    if (Interlocked.CompareExchange(ref recentlyConnected, 2, 1) == 1)
                    {
                        StartAsync().GetAwaiter().GetResult();
                    }
                }
                finally { }
            }
        }

        private void ReceiveWorker(object state)
        {
            var redisClient = state as RedisClient;
            RedisObject obj;

            while (!disposedValue)
            {
                try {
                    while ((obj = RedisObjectDeterminator.Determine(
                               redisClient.ReceiverEnumerator
#if DEBUG
                               ,
                               Configuration.Logger
#endif

                           )) is RedisPushType pushObj) {
                        if (pushObj[0].ToString() == "invalidate") {
                            foreach (var item in pushObj[1] as RedisArray) {
                                InvalidateSubscription(item);
                            }
                        }

                        OnPushMessageReceived?.Invoke(pushObj);
                    }
                } catch (Exception ex) {
                    OnException?.Invoke(ex, ReceiverEnumerator.Socket);

                    continue;
                }
#if DEBUG
                Configuration.Logger($"{nameof(ReceiveWorker)} :emergencyFlag =>{ReceiverEnumerator.EmergencyFlag}, taskCompletionSourcesCount => {redisCompletedTasks.Count}, emergentTaskCompletionSourcesCount =>{emergentRedisCompletedTasks.Count}, obj=> {obj}, isnull => {obj == null}, type => {obj?.GetType().FullName}");
#endif
                TaskCompletionSource<RedisObject> tcs;

                if (ReceiverEnumerator.EmergencyFlag)
                {
                    Configuration.Logger("Receiver Managed Thread Id => " + Thread.CurrentThread.ManagedThreadId);

                    tcs = emergentRedisCompletedTasks.Take();
                }
                else
                {
                    tcs = redisCompletedTasks.Take();
                }
#if DEBUG
                Configuration.Logger($"{nameof(ReceiveWorker)} :");
#endif

                tcs.TrySetResult(obj);
            }
        }

        private byte[][] GetSegments(string[] commandParams)
        {
            var segments = new byte[commandParams.Length * 2 + 1][];

            segments[0] = Encoding.ASCII.GetBytes($"*{commandParams.Length}\r\n");

            for (int i = 1; i < segments.Length; i += 2)
            {
                var commandParam = Encoding.UTF8.GetBytes($"{commandParams[(i - 1) / 2]}\r\n");

                segments[i] = Encoding.ASCII.GetBytes($"${commandParam.Length - 2}\r\n");
                segments[i + 1] = commandParam;
            }

            return segments;
        }

        public async Task<RedisObject> ExecuteAsync(CancellationToken cancellationToken, params string[] commandParams)
        {
            await Task.Yield();

            var segments = GetSegments(commandParams);

            var result = await ExecuteAsync(TaskCreationOptions.RunContinuationsAsynchronously, cancellationToken, segments);

#if DEBUG
            Configuration.Logger($"{nameof(ExecuteAsync)} : Command =>{string.Join(" ", commandParams)}, Result => {result}");
#endif

            return result;
        }

        public RedisObject Execute(CancellationToken cancellationToken, params string[] commandParams)
        {
            var segments = GetSegments(commandParams);

            var result = ExecuteAsync(TaskCreationOptions.None, cancellationToken, segments).ConfigureAwait(false).GetAwaiter().GetResult();

#if DEBUG
            Configuration.Logger($"{nameof(ExecuteAsync)} : Command =>{string.Join(" ", commandParams)}, Result => {result}");
#endif

            return result;
        }

        public async Task<RedisObject> ExecuteAsync(TaskCreationOptions taskCreationOptions, CancellationToken cancellationToken, params byte[][] commandParams)
        {
            while (true)
            {
                var tcs = new TaskCompletionSource<RedisObject>(taskCreationOptions);

                cancellationToken.Register((_tcs) =>
                {
                    (_tcs as TaskCompletionSource<RedisObject>).TrySetCanceled();
                }, tcs);

                var result = ReceiverEnumerator.SendData(commandParams, (transferredBytes) =>
                {
#if DEBUG
                    Configuration.Logger($"{nameof(ExecuteAsync)} : ");
#endif
                    redisCompletedTasks.Add(tcs);
                });

                switch (result)
                {
                    case DnsEndPointTCPConnector.Enumerator.SendResults.CouldntSent:
                    case DnsEndPointTCPConnector.Enumerator.SendResults.PartialSent:
                        cancellationToken.ThrowIfCancellationRequested();
                        //throw new RedisClientNotConectedException(result.ToString());
                        continue;
                    case DnsEndPointTCPConnector.Enumerator.SendResults.Buffered:
                    case DnsEndPointTCPConnector.Enumerator.SendResults.Sent:
                    default:
                        var obj = await tcs.Task.ConfigureAwait(false);

                        if (obj is RedisSimpleError _simpleError)
                        {
                            throw new Exception(_simpleError);
                        }
                        else if (obj is RedisBlobError _blobError)
                        {
                            throw new Exception(_blobError);
                        }

                        return obj;
                }
            }
        }

        public async Task<RedisObject> ExecuteEmergentAsync(params string[] commandParams)
        {
            var tcs = new TaskCompletionSource<RedisObject>(TaskCreationOptions.RunContinuationsAsynchronously);
#if DEBUG
            Configuration.Logger($"{nameof(ExecuteEmergentAsync)} : Command =>{string.Join(" ", commandParams)}");
#endif
            if (!await ReceiverEnumerator.SendEmergentDataAsync(GetSegments(commandParams), () =>
             {
#if DEBUG
                 Configuration.Logger($"{nameof(ExecuteEmergentAsync)} : ");
#endif
                 emergentRedisCompletedTasks.Add(tcs);
             }))
            {
                tcs.TrySetException(new RedisClientNotConectedException("Emergent Sender Couldnt Sent"));
            }

            return await tcs.Task;
        }

        private Task<RedisObject> AuthAsync()
        {
            return ExecuteEmergentAsync("AUTH", Configuration.Password);
        }

        private Task<RedisObject> Hello3Async()
        {
            return ExecuteEmergentAsync("HELLO", "3");
        }

        private Task<RedisObject> ClientTrackingOnAsync()
        {
            return ExecuteEmergentAsync("CLIENT", "TRACKING", "ON");
        }

        public Task<RedisObject> RoleAsync()
        {
            return ExecuteEmergentAsync("ROLE");
        }

        public async Task<bool> CheckMasterAsync()
        {
            return await RoleAsync() is RedisArray infoResult &&
                (infoResult.Items[0] is RedisBlobString || infoResult.Items[0] is RedisSimpleString) &&
                infoResult.Items[0].ToString() == "master";
        }

        public Task<RedisObject> SelectDbAsync(int dbId)
        {
            return ExecuteEmergentAsync("SELECT", dbId.ToString());
        }

        public async Task<T> GetOrUpdateAsync<T>(string key, Func<Task<T>> updateFunc, TimeSpan? expiry = null, CancellationToken cancellationToken = default)
        {
            return (T)await _GetOrUpdateAsync(typeof(T), key, async () => await updateFunc(), cancellationToken, expiry);
        }

        public T GetOrUpdate<T>(string key, Func<T> updateFunc, TimeSpan? expiry = null, CancellationToken cancellationToken = default)
        {
            return (T)_GetOrUpdate(typeof(T), key, () => updateFunc(), cancellationToken, expiry);
        }

        private object GetOrUpdateWithLocalCache(Type type, string key, Func<object> updateFunc, CancellationToken cancellationToken, TimeSpan? expiry = null)
        {
            try
            {
                return keyValuePairs.GetOrAdd(key, (_key) =>
                {
                    if (!IsConnected)
                    {
                        return updateFunc() ?? throw new RequiredParameterException { Key = key, Expiry = expiry };
                    }

                    object result = GetWithoutLocalCache(type, key, cancellationToken);

                    if (result != default) { return result; }

                    var obj = updateFunc();

                    if (obj == null)
                    {
                        throw new RequiredParameterException { Key = key, Expiry = expiry };
                    }

                    string objStr;

                    if (obj is string _objStr)
                    {
                        objStr = _objStr;
                    }
                    else
                    {
                        objStr = JsonConvert.SerializeObject(obj);
                    }

                    Set(key, objStr, expiry);
                    GetWithoutLocalCache(key, cancellationToken); // for invalidate message

                    return obj;
                });
            }
            catch (RequiredParameterException ex)
            {
                OnException?.Invoke(ex, ReceiverEnumerator.Socket);

                return default;
            }
            catch (TaskCanceledException ex)
            {
                OnException?.Invoke(ex, ReceiverEnumerator.Socket);

                return updateFunc();
            }
        }

        private object GetOrUpdateWithoutLocalCache(Type type, string key, Func<object> updateFunc, CancellationToken cancellationToken, TimeSpan? expiry = null)
        {
            if (!IsConnected)
            {
                return updateFunc();
            }

            try
            {
                object result = GetWithoutLocalCache(type, key, cancellationToken);

                if (result != default) { return result; }

                var obj = updateFunc();

                if (obj == null)
                {
                    return default;
                }

                string objStr;

                if (obj is string _objStr)
                {
                    objStr = _objStr;
                }
                else
                {
                    objStr = JsonConvert.SerializeObject(obj);
                }

                Set(key, objStr, expiry);

                return obj;
            }
            catch (TaskCanceledException ex)
            {
                OnException?.Invoke(ex, ReceiverEnumerator.Socket);

                return updateFunc();
            }
        }

        private async Task<object> GetOrUpdateWithLocalCacheAsync(Type type, string key, Func<Task<object>> updateFunc, CancellationToken cancellationToken, TimeSpan? expiry = null)
        {
            try
            {
                return await keyValuePairs.GetOrAddAsync(key, async (_key) =>
                {
                    if (!IsConnected)
                    {
                        return await updateFunc() ?? throw new RequiredParameterException { Key = key, Expiry = expiry };
                    }

                    object result = await GetWithoutLocalCacheAsync(type, key, cancellationToken);

                    if (result != default) { return result; }

                    var obj = await updateFunc();

                    if (obj == null)
                    {
                        throw new RequiredParameterException { Key = key, Expiry = expiry };
                    }

                    string objStr;

                    if (obj is string _objStr)
                    {
                        objStr = _objStr;
                    }
                    else
                    {
                        objStr = JsonConvert.SerializeObject(obj);
                    }

                    await SetAsync(key, objStr, expiry);
                    GetWithoutLocalCacheAsync(key, cancellationToken).ConfigureAwait(false); // for invalidate message

                    return obj;
                });
            }
            catch (RequiredParameterException ex)
            {
                OnException?.Invoke(ex, ReceiverEnumerator.Socket);

                return default;
            }
            catch (TaskCanceledException ex)
            {
                OnException?.Invoke(ex, ReceiverEnumerator.Socket);

                return await updateFunc();
            }
        }

        private async Task<object> GetOrUpdateWithoutLocalCacheAsync(Type type, string key, Func<Task<object>> updateFunc, CancellationToken cancellationToken, TimeSpan? expiry = null)
        {
            if (!IsConnected)
            {
                return await updateFunc();
            }

            try
            {
                object result = await GetWithoutLocalCacheAsync(type, key, cancellationToken);

                if (result != default) { return result; }

                var obj = await updateFunc();

                if (obj == null)
                {
                    return default;
                }

                string objStr;

                if (obj is string _objStr)
                {
                    objStr = _objStr;
                }
                else
                {
                    objStr = JsonConvert.SerializeObject(obj);
                }

                await SetAsync(key, objStr, expiry);

                return obj;
            }
            catch (TaskCanceledException ex)
            {
                OnException?.Invoke(ex, ReceiverEnumerator.Socket);

                return await updateFunc();
            }
        }

        private string GetWithLocalCache(string key, CancellationToken cancellationToken)
        {
            return (string)keyValuePairs.GetOrAdd(key, (_key) => GetWithoutLocalCache(_key, cancellationToken));
        }

        private object GetWithLocalCache(Type type, string key, CancellationToken cancellationToken)
        {
            return keyValuePairs.GetOrAdd(key, (_key) => GetWithoutLocalCache(type, key, cancellationToken));
        }

        private string GetWithoutLocalCache(string key, CancellationToken cancellationToken)
        {
            var obj = Execute(cancellationToken, "GET", key);

            if (obj is RedisBlobString _blob)
            {
                return _blob;
            }
            else if (obj is RedisSimpleString _simple)
            {
                return _simple;
            }
            else if (obj is RedisNull)
            {
                return null;
            }

            return null;
        }

        private object GetWithoutLocalCache(Type type, string key, CancellationToken cancellationToken)
        {
            string result = GetWithoutLocalCache(key, cancellationToken);

            if (result == null)
            {
                return default;
            }

            if (type == typeof(string))
            {
                return result;
            }

            return JsonConvert.DeserializeObject(result, type);
        }

        public string Get(string key, CancellationToken cancellationToken = default)
        {
            return _GetAsString(key, cancellationToken);
        }

        public T Get<T>(string key, CancellationToken cancellationToken = default)
        {
            return (T)_Get(typeof(T), key, cancellationToken);
        }

        private async Task<string> GetWithLocalCacheAsync(string key, CancellationToken cancellationToken)
        {
            return (string)await keyValuePairs.GetOrAddAsync(key, async (_key) => await GetWithoutLocalCacheAsync(key, cancellationToken));
        }

        private Task<object> GetWithLocalCacheAsync(Type type, string key, CancellationToken cancellationToken)
        {
            return keyValuePairs.GetOrAddAsync(key, (_key) => GetWithoutLocalCacheAsync(type, key, cancellationToken));
        }

        private async Task<string> GetWithoutLocalCacheAsync(string key, CancellationToken cancellationToken)
        {
            RedisObject obj = await ExecuteAsync(cancellationToken, "GET", key);

            if (obj is RedisBlobString _blob)
            {
                return _blob;
            }
            else if (obj is RedisSimpleString _simple)
            {
                return _simple;
            }
            else if (obj is RedisNull)
            {
                return null;
            }

            return null;
        }

        private async Task<object> GetWithoutLocalCacheAsync(Type type, string key, CancellationToken cancellationToken)
        {
            string result = await GetWithoutLocalCacheAsync(key, cancellationToken);

            if (result == null)
            {
                return default;
            }

            if (type == typeof(string))
            {
                return result;
            }

            return JsonConvert.DeserializeObject(result, type);
        }

        public Task<string> GetAsync(string key, CancellationToken cancellationToken = default)
        {
            return _GetAsStringAsync(key, cancellationToken);
        }

        public async Task<T> GetAsync<T>(string key, CancellationToken cancellationToken = default)
        {
            return (T)await _GetAsync(typeof(T), key, cancellationToken);
        }

        public Task<bool> SetAsync<T>(string key, T data, TimeSpan? expiry, CancellationToken cancellationToken = default)
        {
            return SetAsync(key, JsonConvert.SerializeObject(data), expiry, cancellationToken);
        }

        public async Task<bool> SetAsync(string key, string data, TimeSpan? expiry, CancellationToken cancellationToken = default)
        {
            if (data == null)
            {
                throw new RequiredParameterException(nameof(data));
            }

            RedisObject obj;

            if (expiry.HasValue)
            {
                obj = await ExecuteAsync(cancellationToken, "SET", key, data, "EX", ((int)expiry.Value.TotalSeconds).ToString());
            }
            else
            {
                obj = await ExecuteAsync(cancellationToken, "SET", key, data);
            }

            if (obj is RedisSimpleString _simpleString)
            {
                return _simpleString == "OK";
            }
            else if (obj is RedisBlobString _blobString)
            {
                return _blobString == "OK";
            }

            return false;
        }

        public bool Set<T>(string key, T data, TimeSpan? expiry, CancellationToken cancellationToken = default)
        {
            return Set(key, JsonConvert.SerializeObject(data), expiry, cancellationToken);
        }

        public bool Set(string key, string data, TimeSpan? expiry, CancellationToken cancellationToken = default)
        {
            if (data == null)
            {
                throw new RequiredParameterException(nameof(data));
            }

            RedisObject obj;

            if (expiry.HasValue)
            {
                obj = Execute(cancellationToken, "SET", key, data, "EX", ((int)expiry.Value.TotalSeconds).ToString());
            }
            else
            {
                obj = Execute(cancellationToken, "SET", key, data);
            }

            if (obj is RedisSimpleString _simpleString)
            {
                return _simpleString == "OK";
            }
            else if (obj is RedisBlobString _blobString)
            {
                return _blobString == "OK";
            }

            return false;
        }

        public async Task<long> ClearAsync(CancellationToken cancellationToken, params string[] keys)
        {
            return (RedisNumber)await ExecuteAsync(cancellationToken, new string[] { "DEL" }.Concat(keys).ToArray());
        }

        public Task<long> ClearAsync(params string[] keys)
        {
            return ClearAsync(CancellationToken.None, keys);
        }

        public static Task<RedisClient> CreateClientAsync(string connectionString)
        {
            var configuration = new RedisConfiguration(connectionString);

            return CreateClientAsync(configuration);
        }

        public static async Task<RedisClient> CreateClientAsync(RedisConfiguration configuration, Action<Exception> onException = null)
        {
            try
            {
                var client = new RedisClient(configuration);

                await client.StartAsync();

                return client;
            }
            catch (Exception ex)
            {
                onException?.Invoke(ex);
            }

            return null;
        }

        private void InvalidateSubscription(RedisObject obj)
        {
            if (Configuration.ClientCache)
            {
                if (obj is RedisNull)
                {
                    keyValuePairs.Clear();
                }
                else if (obj is RedisSimpleString redisSimpleString)
                {
                    keyValuePairs.TryRemove(redisSimpleString, out _);
                }
                else if (obj is RedisBlobString redisBlobString)
                {
                    keyValuePairs.TryRemove(redisBlobString, out _);
                }
            }
        }

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    ReceiverEnumerator.Dispose();
                }

                ReceiverEnumerator = null;

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        class RedisCompletedAsyncResult<T> : IAsyncResult, IDisposable
        {
            private readonly object state;
            private readonly Action<IAsyncResult> completeCallBack;
            private volatile int isExecuted = 0;

            ManualResetEvent resetEvent = new ManualResetEvent(false);

            public RedisCompletedAsyncResult(object state, Action<IAsyncResult> completeCallBack)
            {
                this.state = state;
                this.completeCallBack = completeCallBack;
            }

            public T Data { get; private set; }

            public void OnComplete(T data)
            {
                this.Data = data;
                this.IsCompleted = true;

                resetEvent.Set();

                if (Interlocked.Increment(ref isExecuted) == 1)
                {
                    completeCallBack?.Invoke(this);
                }
            }

            public void ExitSync()
            {
                Interlocked.Exchange(ref isExecuted, 2);
            }

            #region IAsyncResult Members

            public object AsyncState { get { return state; } }

            public WaitHandle AsyncWaitHandle { get { return resetEvent; } }

            public bool CompletedSynchronously { get { return isExecuted == 1; } }

            public bool IsCompleted { get; private set; }

            #endregion

            public void Dispose()
            {
                resetEvent.Dispose();
            }
        }
    }
}
