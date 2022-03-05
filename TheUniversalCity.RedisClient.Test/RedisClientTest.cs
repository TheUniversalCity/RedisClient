using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TheUniversalCity.RedisClient.RedisObjects.Agregates;
using Xunit;
using Xunit.Abstractions;

namespace TheUniversalCity.RedisClient.Test
{
    public class RedisClientTest : IClassFixture<OutputLog>
    {
        public const string CONNECTION_LOCAL = "localhost,allowAdmin=true,clientCache=true";
        public const string TEST_KEY = "Fıstıkçı Şahap";
        public string TEST_DATA = string.Concat(Enumerable.Range(0, 1000).Select(c => "X"));


        public RedisClientTest(ITestOutputHelper output, OutputLog outputLog)
        {
            var writer = new XUnitTextWriter(output, outputLog.DiagnosticMessageSink);
            Console.SetOut(writer);
            var x = ThreadPool.SetMinThreads(200, 200);
        }

        [Fact]
        [Trait("Category", "RedisClientBenchmark")]
        public async Task RedisClientParallelCall()
        {
            //,sendBufferSize=524288,receiveBufferSize=524288
            var sw = new Stopwatch();

            var redisClient1 = await RedisClient.CreateClientAsync("localhost,allowAdmin=true,clientCache=false");
            var taskList = new List<Task>();

            sw.Start();

            var cts = new CancellationTokenSource(8000);

            for (int i = 0; i < 10000; i++)
            {
                //taskList.Add(Task.Factory.StartNew(() =>
                //{
                //    var value = redisClient1.Get(TEST_KEY);
                //}));

                taskList.Add(redisClient1.GetAsync(TEST_KEY, cts.Token));
            }

            await Task.WhenAll(taskList);

            sw.Stop();

            Console.WriteLine($"Time elapsed : {sw.ElapsedMilliseconds}");
        }

        [Fact]
        [Trait("Category", "StackExchangeBenchmark")]
        public async Task StackExchangeParallelCall()
        {
            var sw = new Stopwatch();

            var connection = StackExchange.Redis.ConnectionMultiplexer.Connect("localhost,allowAdmin=true");
            var db = connection.GetDatabase();
            var taskList = new List<Task>();

            sw.Start();

            for (int i = 0; i < 10000; i++)
            {
                //taskList.Add(await Task.Factory.StartNew(async () =>
                //{
                //    string value = await db.StringGetAsync(TEST_KEY);
                //}));

                taskList.Add(db.StringGetAsync(TEST_KEY));
            }

            await Task.WhenAll(taskList);

            sw.Stop();

            Console.WriteLine($"Time elapsed : {sw.ElapsedMilliseconds}");
        }

        [Fact]
        [Trait("Category", "RedisClientBenchmark")]
        public async Task RedisClientSynchronCall()
        {
            var sw = new Stopwatch();

            var redisClient1 = await RedisClient.CreateClientAsync("localhost,allowAdmin=true,clientCache=false");

            //await redisClient1.Connect();
            sw.Start();

            for (int i = 0; i < 1000; i++)
            {
                var value = await redisClient1.GetAsync(TEST_KEY);
            }

            //var value1 = redisClient1.Get(TEST_KEY);

            //Thread.Sleep(10000);

            //var value2 = redisClient1.Get(TEST_KEY);

            //Assert.Equal(value1, value2);

            sw.Stop();

            Console.WriteLine($"Time elapsed : {sw.ElapsedMilliseconds}");
        }

        [Fact]
        [Trait("Category", "StackExchangeBenchmark")]
        public async Task StackExchangeSynchronCall()
        {
            var sw = new Stopwatch();

            var connection = await StackExchange.Redis.ConnectionMultiplexer.ConnectAsync("localhost,allowAdmin=true");
            var db = connection.GetDatabase();

            sw.Start();

            for (int i = 0; i < 1000; i++)
            {
                string value = db.StringGet(TEST_KEY);
            }

            sw.Stop();

            Console.WriteLine($"Time elapsed : {sw.ElapsedMilliseconds}");
            //redisClient1.Set("Localization:ECommerceBackend:tr-TR:bo_IsThereAnyStoreStock", "test", TimeSpan.FromDays(1));
        }

        [Fact]
        [Trait("Category", "RedisClientBenchmark")]
        public async Task RedisClientParallelSetCall()
        {
            var sw = new Stopwatch();

            var redisClient1 = await RedisClient.CreateClientAsync("localhost,allowAdmin=true,clientCache=true");
            var taskList = new List<Task>();
            var randomString = Guid.NewGuid().ToString();

            sw.Start();

            for (int i = 0; i < 1; i++)
            {
                for (int j = 0; j < 1000; j++)
                {
                    taskList.Add(redisClient1.SetAsync(TEST_KEY, TEST_DATA, null));
                }

                await Task.WhenAll(taskList);

                taskList.Clear();
            }

            sw.Stop();

            Console.WriteLine($"Time elapsed : {sw.ElapsedMilliseconds}");
        }

        [Fact]
        [Trait("Category", "StackExchangeBenchmark")]
        public async Task StackExchangeParallelSetCall()
        {
            var sw = new Stopwatch();

            var connection = await StackExchange.Redis.ConnectionMultiplexer.ConnectAsync("localhost,allowAdmin=true");
            var db = connection.GetDatabase();
            var taskList = new List<Task>();
            var randomString = Guid.NewGuid().ToString();

            sw.Start();

            for (int j = 0; j < 1; j++)
            {
                for (int i = 0; i < 1000; i++)
                {
                    taskList.Add(Task.Factory.StartNew(() =>
                    {
                        bool value = db.StringSet(TEST_KEY, TEST_DATA, null);
                    }));
                }

                await Task.WhenAll(taskList);

                taskList.Clear();
            }

            sw.Stop();

            Console.WriteLine($"Time elapsed : {sw.ElapsedMilliseconds}");
            //redisClient1.Set("Localization:ECommerceBackend:tr-TR:bo_IsThereAnyStoreStock", "test", TimeSpan.FromDays(1));
        }

        [Fact]
        [Trait("Category", "RedisClientBenchmark")]
        public async Task RedisClientSynchronSetCall()
        {
            var sw = new Stopwatch();

            var redisClient1 = await RedisClient.CreateClientAsync("localhost,allowAdmin=true,clientCache=true,db=1");
            var randomString = Guid.NewGuid().ToString();

            sw.Start();

            for (int i = 0; i < 1000; i++)
            {
                var value = await redisClient1.SetAsync(TEST_KEY, TEST_KEY, null);
            }

            sw.Stop();

            Console.WriteLine($"Time elapsed : {sw.ElapsedMilliseconds}");
        }

        [Fact]
        [Trait("Category", "StackExchangeBenchmark")]
        public async Task StackExchangeSynchronSetCall()
        {
            var sw = new Stopwatch();

            var connection = await StackExchange.Redis.ConnectionMultiplexer.ConnectAsync("localhost,allowAdmin=true");
            var db = connection.GetDatabase();
            var randomString = Guid.NewGuid().ToString();

            sw.Start();

            for (int i = 0; i < 1000; i++)
            {
                bool value = db.StringSet(TEST_KEY, TEST_KEY, null);
            }

            sw.Stop();

            Console.WriteLine($"Time elapsed : {sw.ElapsedMilliseconds}");
            //redisClient1.Set("Localization:ECommerceBackend:tr-TR:bo_IsThereAnyStoreStock", "test", TimeSpan.FromDays(1));
        }

        [Fact]
        public async Task RedisClientClientCacheInvalidate()
        {
            var sw = new Stopwatch();

            var redisClient1 = await RedisClient.CreateClientAsync("localhost,allowAdmin=true,clientCache=false");
            var redisClient2 = await RedisClient.CreateClientAsync("localhost,allowAdmin=true,clientCache=true");
            var messageCount = 0;

            redisClient2.OnPushMessageReceived += (redisObj) =>
            {
                if (redisObj[0].ToString() == "invalidate")
                {
                    foreach (var item in redisObj[1] as RedisArray)
                    {
                        Assert.Equal(item.ToString(), TEST_KEY);

                        messageCount++;
                    }
                }
            };

            sw.Start();

            var randomString = Guid.NewGuid().ToString();

            if (await redisClient1.SetAsync(TEST_KEY, randomString, null))
            {
                var value = redisClient2.Get(TEST_KEY);

                Assert.Equal(randomString, value);

                var randomString2 = Guid.NewGuid().ToString();

                if (await redisClient1.SetAsync(TEST_KEY, randomString2, null))
                {
                    Thread.Sleep(50);

                    var value2 = redisClient2.Get(TEST_KEY);

                    Assert.Equal(randomString2, value2);
                }
            }

            Assert.Equal(1, messageCount);

            sw.Stop();

            Console.WriteLine($"Time elapsed : {sw.ElapsedMilliseconds}");
        }

        [Fact]
        public async Task RedisClientEncoding()
        {
            var sw = new Stopwatch();

            var redisClient = await RedisClient.CreateClientAsync("localhost,allowAdmin=true,clientCache=true");
            var value = default(string);

            if (await redisClient.SetAsync(TEST_KEY, TEST_KEY, null))
            {
                value = redisClient.Get(TEST_KEY);
            }

            Assert.Equal(TEST_KEY, value);

            sw.Stop();

            Console.WriteLine($"Time elapsed : {sw.ElapsedMilliseconds}");
        }

        [Fact]
        public async Task RedisClientClearCache()
        {
            var sw = new Stopwatch();

            var redisClient = await RedisClient.CreateClientAsync("localhost,allowAdmin=true,clientCache=true");

            await redisClient.SetAsync(TEST_KEY, TEST_KEY, null);

            Console.WriteLine("Not Locked");

            var count = await redisClient.ClearAsync(TEST_KEY);

            Assert.Equal(1, count);

            sw.Stop();

            Console.WriteLine($"Time elapsed : {sw.ElapsedMilliseconds}");
        }

        [Fact]
        public async Task RedisClientBulkParallelInsertAndParallelGetAndCrossCheck()
        {
            var sw = new Stopwatch();
            var redisClient = await RedisClient.CreateClientAsync("localhost,allowAdmin=true,clientCache=true");
            var taskList = new List<Task>();

            for (int i = 0; i < 1000; i++)
            {
                taskList.Add(Task.Factory.StartNew((_i) => redisClient.SetAsync(TEST_KEY + _i, _i, null), i));
            }

            await Task.WhenAll(taskList);

            taskList.Clear();

            await redisClient.SetAsync(TEST_KEY, "-1", null);

            var xxx1 = await redisClient.GetAsync(TEST_KEY);

            for (int i = 0; i < 50; i++)
            {
                taskList.Add(Task.Factory.StartNew(async (_i) =>
                {
                    var x = int.Parse(await redisClient.GetAsync(TEST_KEY + _i));

                    Assert.Equal(_i, x);
                }, i));
            }

            redisClient.ReceiverEnumerator.Socket.Shutdown(System.Net.Sockets.SocketShutdown.Both);

            var xxx = await redisClient.GetAsync(TEST_KEY);

            Assert.Equal(xxx1, xxx);

            var allTask = Task.WhenAll(taskList.ToArray());

            try
            {
                //Task.WaitAll(taskList.ToArray());
                await allTask;
            }
            catch (Exception ex)
            {
                foreach (var _ex in allTask.Exception.InnerExceptions)
                {
                    Console.WriteLine(_ex.Message);
                }
            }

            //await Assert.ThrowsAnyAsync<SocketException>(async () => await Task.WhenAll(taskList));

            sw.Stop();

            Console.WriteLine($"Time elapsed : {sw.ElapsedMilliseconds}");
        }


        [Fact]
        public async Task RedisClientGetBigData()
        {
            var sw = new Stopwatch();

            var redisClient = await RedisClient.CreateClientAsync("localhost,allowAdmin=true,clientCache=true");

            var randomString = string.Concat(Enumerable.Range(0, 500000).Select(c => 'X'));
            var y = await redisClient.ClearAsync(Enumerable.Range(0, 100).Select(c => $"favorite-option:xxx").ToArray());
            var taskList = new List<Task>();

            sw.Start();

            for (int i = 0; i < 1; i++)
            {
                for (int j = 0; j < 1000; j++)
                {
                    taskList.Add(Task.Factory.StartNew(() =>
                    {
                        redisClient.GetOrUpdateAsync($"favorite-option:xxx", () =>
                        {
                            return Task.FromResult("rertretretrete");
                        }, null).ConfigureAwait(false);//.GetAwaiter().GetResult();
                    }));
                }

                await Task.WhenAll(taskList);

                taskList.Clear();
            }

            sw.Stop();

            Console.WriteLine($"Time elapsed : {sw.ElapsedMilliseconds}");
        }

        [Fact]
        public async Task RedisClientGetBigDataAsync()
        {
            var sw = new Stopwatch();

            var redisClient = await RedisClient.CreateClientAsync("localhost,allowAdmin=true,clientCache=true");

            var y = await redisClient.ClearAsync(Enumerable.Range(0, 100).Select(c => $"favorite-option:xxx").ToArray());
            var taskList = new List<Task>();

            sw.Start();

            for (int i = 0; i < 1; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    taskList.Add(Task.Factory.StartNew(() =>
                    redisClient.GetOrUpdateAsync($"favorite-option:xxx", async () =>
                    {
                        //await Task.Delay(1000);
                        return await Task.FromResult(string.Concat(Enumerable.Range(0, 500000).Select(c => 'X')));
                    }, null)));
                }

                await Task.WhenAll(taskList);

                taskList.Clear();
            }

            sw.Stop();

            Console.WriteLine($"Time elapsed : {sw.ElapsedMilliseconds}");
        }


        [Fact]
        public async Task GetOrUpdateTest()
        {
            var sw = new Stopwatch();

            var redisClient = await RedisClient.CreateClientAsync("localhost,allowAdmin=true,clientCache=true");

            var randomString = Guid.NewGuid().ToString();
            var cts = new CancellationTokenSource(8000);
            var taskList = new List<Task>();

            sw.Start();

            for (int i = 0; i < 1000; i++)
            {
                //taskList.Add(Task.Factory.StartNew(()=> redisClient.GetOrUpdateAsync(randomString, () => Task.FromResult(1), TimeSpan.FromMinutes(1), cts.Token)));

                taskList.Add(Task.Factory.StartNew(() => 
                redisClient.GetOrUpdate(randomString, () => Task.FromResult(1), TimeSpan.FromMinutes(1), cts.Token)
                ));

            }

            sw.Stop();

            await Task.WhenAll(taskList);

            Console.WriteLine($"Time elapsed : {sw.ElapsedMilliseconds}");
        }

        [Fact]
        public void CpuPerformans()
        {
            var sw = new Stopwatch();

            sw.Start();

            for (int i = 0; i < 1000000; i++)
            {
                var randomString = Guid.NewGuid().ToString();
            }

            sw.Stop();

            Console.WriteLine($"Time elapsed : {sw.ElapsedMilliseconds}");
        }

        [Fact]
        public void ManuelResetEventPerformance()
        {
            var sw = new Stopwatch();
            var e = new ManualResetEvent(false);
            var bc = new BlockingCollection<int>();
            var result = 0;

            e.Set();

            for (int i = 0; i < 10000; i++)
            {
                bc.Add(i);
            }

            sw.Start();

            for (int i = 0; i < 10000; i++)
            {
                if (true)
                {
                    try
                    {
                        e.WaitOne();
                        e.WaitOne();
                        e.WaitOne();
                        result = bc.Take();// emergentCancellationToken.Token);
                    }
                    catch (Exception)
                    {
                        result = bc.Take();
                    }
                }//e.WaitOne();
            }

            sw.Stop();

            Console.WriteLine($"Time elapsed : {sw.ElapsedMilliseconds}");
        }
    }
}
