using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using TheUniversalCity.RedisClient.Exceptions;

namespace TheUniversalCity.RedisClient.Streaming
{
    public sealed class DnsEndPointTCPConnector : IEnumerable<byte>
    {
        private const int DEFAULT_RECEIVER_BUFFER_SIZE = short.MaxValue;
        private const int DEFAULT_SEND_BUFFER_SIZE = short.MaxValue;
        private const int DEFAULT_CONNECT_RETRY_COUNT = 3;
        private const int DEFAULT_CONNECT_RETRY_INTERVAL = 300;

        public event Action<Exception, Enumerator> OnException;
        public event Action<DnsEndPoint, Enumerator> OnConnectionTryFailed;
        public event Action<Enumerator> OnConnected;
        public event Action<Exception, Enumerator> OnReceiverDisconnect;
        public event Action<Exception, Enumerator> OnSenderDisconnect;

        public DnsEndPointTCPConnector(
            DnsEndPoint[] endPointList,
            int receiverBufferSize = DEFAULT_RECEIVER_BUFFER_SIZE,
            int sendBufferSize = DEFAULT_SEND_BUFFER_SIZE,
            int connectRetry = DEFAULT_CONNECT_RETRY_COUNT,
            int connectRetryInterval = DEFAULT_CONNECT_RETRY_INTERVAL)
        {
            EndPointList = endPointList;
            ReceiverBufferSize = receiverBufferSize;
            SendBufferSize = sendBufferSize;
            ConnectRetry = connectRetry;
            ConnectRetryInterval = connectRetryInterval;
        }

        public DnsEndPoint[] EndPointList { get; }
        public int ReceiverBufferSize { get; }
        public int SendBufferSize { get; }
        public int ConnectRetry { get; }
        public int ConnectRetryInterval { get; }

        public IEnumerator<byte> GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator(this);
        }

        public sealed class Enumerator : IEnumerator<byte>, IDisposable
        {
            public enum SendResults
            {
                CouldntSent = -1,
                Buffered,
                PartialSent,
                Sent
            }

            private volatile int threadCounter;
            public volatile int bufferedMessageCounter;

            private int selectedEndpointIndex;

            readonly ManualResetEvent receivedSignalEvent = new ManualResetEvent(false);
            readonly ManualResetEvent sentSignalEvent = new ManualResetEvent(false);
            readonly ManualResetEvent senderCompletedSignalEvent = new ManualResetEvent(false);
            readonly ManualResetEvent connectSignalEvent = new ManualResetEvent(false);

            readonly ManualResetEvent emergentSentSignalEvent = new ManualResetEvent(false);
            readonly ManualResetEvent emergentCompletedSignalEvent = new ManualResetEvent(false);

            private volatile bool _emergencyFlag = false;

            public bool EmergencyFlag { get { return _emergencyFlag; } }

            public Socket Socket { get; private set; }
            private readonly byte[] receiverBuffer;
            private readonly byte[] senderBuffer;
            private readonly byte[] emergentSenderBuffer;

            readonly SocketAsyncEventArgs readEventArgs;
            readonly SocketAsyncEventArgs writeEventArgs;
            readonly SocketAsyncEventArgs emergentWriteEventArgs;

            private int receiverOffset;
            private int receiverDataLength;
            private int senderOffset;
            private int senderDataLength;
            private readonly object moveNextLckObj = new object();
            private readonly object connectLckObj = new object();
            private readonly object emergencySendLckObj = new object();

            private readonly object sendLckObj = new object();
            private readonly AutoResetEvent sendAutoResetEvent = new AutoResetEvent(true);

            private byte _current;
            private int currentRetryCount;
            private bool _disposed;

            public event Action OnEmergentStart;
#pragma warning disable CS0067 // The event 'DnsEndPointTCPConnector.Enumerator.OnEmergentEnd' is never used
            public event Action OnEmergentEnd;
#pragma warning restore CS0067 // The event 'DnsEndPointTCPConnector.Enumerator.OnEmergentEnd' is never used

            public byte Current => _current;

            public DnsEndPointTCPConnector Receiver { get; }

            object IEnumerator.Current => _current;

            public Enumerator(DnsEndPointTCPConnector receiver)
            {
                Receiver = receiver;

                receiverBuffer = new byte[receiver.ReceiverBufferSize];
                senderBuffer = new byte[receiver.SendBufferSize];
                emergentSenderBuffer = new byte[receiver.SendBufferSize];

                readEventArgs = new SocketAsyncEventArgs();
                writeEventArgs = new SocketAsyncEventArgs();
                emergentWriteEventArgs = new SocketAsyncEventArgs();

                readEventArgs.Completed += ReadEventArgs_Completed;
                writeEventArgs.Completed += WriteEventArgs_Completed;
                emergentWriteEventArgs.Completed += EmergentWriteEventArgs_Completed;

                readEventArgs.SetBuffer(receiverBuffer, 0, receiver.ReceiverBufferSize);
                writeEventArgs.SetBuffer(senderBuffer, 0, receiver.SendBufferSize);
                emergentWriteEventArgs.SetBuffer(emergentSenderBuffer, 0, receiver.SendBufferSize);

                emergentCompletedSignalEvent.Reset();
                senderCompletedSignalEvent.Set();

                Connect();
            }

            void SetKeepAlive(uint keepAliveTime, uint keepAliveInterval)
            {
                int size = Marshal.SizeOf(new uint());
                var inOptionValues = new byte[size * 3];
                var on = true;

                BitConverter.GetBytes((uint)(on ? 1 : 0)).CopyTo(inOptionValues, 0);
                BitConverter.GetBytes(keepAliveTime).CopyTo(inOptionValues, size);
                BitConverter.GetBytes(keepAliveInterval).CopyTo(inOptionValues, size * 2);

                Socket.IOControl(IOControlCode.KeepAliveValues, inOptionValues, null);
            }

            private void WriteEventArgs_Completed(object sender, SocketAsyncEventArgs e)
            {
                sentSignalEvent.Set();
            }

            private void EmergentWriteEventArgs_Completed(object sender, SocketAsyncEventArgs e)
            {
                emergentSentSignalEvent.Set();
            }

            private void ReadEventArgs_Completed(object sender, SocketAsyncEventArgs e)
            {
                receivedSignalEvent.Set();
            }

            public bool MoveNext()
            {
                if (receiverDataLength == 0 || receiverDataLength == receiverOffset)
                {
                    lock (moveNextLckObj)
                    {
                        if (receiverDataLength == 0 || receiverDataLength == receiverOffset)
                        {
                            ReceiveBuffer();
                        }
                    }
                }

                _current = receiverBuffer[receiverOffset++];
                return true;
            }

            private int Receive()
            {
                var transferredBytes = 0;

                try
                {
                    transferredBytes = Socket.Receive(receiverBuffer);
                }
                catch (Exception ex)
                {
                    Receiver.OnException?.Invoke(new RedisClientNotConectedException("Not Connected", ex), this);
                }

                receiverOffset = 0;
                receiverDataLength = transferredBytes;

                return transferredBytes;
            }

            private bool ReceiveBuffer()
            {
                int receivedDataLength;
                int counter = 0;

                while ((receivedDataLength = Receive()) == 0)
                {
                    if (++counter == Receiver.ConnectRetry)
                    {
                        var exception = new RedisClientNotConectedException("Not Connected", new SocketException((int)SocketError.ConnectionReset));

                        Reset(false, false);

                        Receiver.OnReceiverDisconnect?.Invoke(exception, this);

                        Console.WriteLine("Not Connected on Receive Buffer");

                        Connect();

                        throw exception;
                    }
                }

                return true;
            }

            private int Send(SocketAsyncEventArgs e, ManualResetEvent manualResetEvent)
            {
                manualResetEvent.Reset();

                if (Socket.SendAsync(e))
                {
                    manualResetEvent.WaitOne();
                }
#if DEBUG
                Console.WriteLine($"{nameof(Send)} Enter, senderOffset => {e.Offset}, BytesTransferred => {e.BytesTransferred}");
#endif
                if (e.SocketError != SocketError.Success)
                {
                    var exception = new RedisClientNotConectedException("Not Connected", new SocketException((int)e.SocketError));
#if DEBUG
                    Console.WriteLine("Send Exception: " + exception.Message);
#endif
                    Receiver.OnException?.Invoke(exception, this);
                }

                return e.BytesTransferred;
            }

            private SendResults SendBuffer()
            {
                var socket = Socket;
                int sentDataLength;
                int counter = 0;
                SendResults result = SendResults.CouldntSent;

                writeEventArgs.SetBuffer(senderOffset, senderDataLength);

                while ((sentDataLength = Send(writeEventArgs, sentSignalEvent)) < senderDataLength)
                {
                    senderOffset += sentDataLength;
                    senderDataLength -= sentDataLength;

                    writeEventArgs.SetBuffer(senderOffset, senderDataLength);

                    if (sentDataLength > 0)
                    {
                        result = SendResults.PartialSent;
                    }

                    if (socket == Socket)
                    {
                        if (sentDataLength == 0 && ++counter == Receiver.ConnectRetry)
                        {
                            connectSignalEvent.Reset();

                            Receiver.OnSenderDisconnect?.Invoke(new RedisClientNotConectedException("Not Connected"), this);

                            Console.WriteLine("Not Connected on Send Buffer");
                            //connectSignalEvent.WaitOne();

                            //Thread.Sleep(300);

                            //throw new Exception("Not Connected");
                            return result;
                        }

                        continue;
                    }

                    //throw new Exception("Not Connected");
                    return result;
                }

                senderOffset = 0;
                senderDataLength = 0;

                Interlocked.Exchange(ref bufferedMessageCounter, 0);

                return SendResults.Sent;
            }

            private bool AddBuffer(byte[][] dataArr)
            {
                var length = 0;

                for (int i = 0; i < dataArr.Length; i++)
                {
                    length += dataArr[i].Length;
                }

                if (senderDataLength + senderOffset + length > senderBuffer.Length)
                {
                    return false;
                }

                var offset = 0;

                for (int i = 0; i < dataArr.Length; i++)
                {
                    var arr = dataArr[i];

                    Buffer.BlockCopy(arr, 0, senderBuffer, senderOffset + senderDataLength + offset, arr.Length);
                    offset += arr.Length;
                }

                Interlocked.Add(ref senderDataLength, length);

                return true;
            }

            private bool AddBuffer(byte[] data, int offset, int length)
            {
                if (senderDataLength + senderOffset + length > senderBuffer.Length)
                {
                    return false;
                }

                Buffer.BlockCopy(data, offset, senderBuffer, senderOffset + senderDataLength, length);

                Interlocked.Add(ref senderDataLength, length);

                return true;
            }

            public SendResults SendData(byte[][] dataArr, Action<uint?> onSentAction = null)
            {
                Interlocked.Increment(ref threadCounter);

                sendAutoResetEvent.WaitOne();
                try
                {
                    if (Interlocked.Decrement(ref threadCounter) != 0 && AddBuffer(dataArr))
                    {
                        Interlocked.Increment(ref bufferedMessageCounter);
                        onSentAction?.Invoke(0);

                        return SendResults.Buffered;
                    }

                    emergentCompletedSignalEvent.WaitOne();
                    senderCompletedSignalEvent.Reset();

                    SendResults result;

                    if (senderDataLength > 0)
                    {
                        result = SendBuffer();

                        if (result == SendResults.CouldntSent)
                        {
                            return SendResults.CouldntSent;
                        }

                        if (result == SendResults.PartialSent)
                        {
                            return SendResults.PartialSent;
                        }
                    }

                    //instantlysend:
                    result = SendResults.CouldntSent;

                    if (AddBuffer(dataArr))
                    {
                        result = SendBuffer();

                        if (result != SendResults.Sent)
                        {
                            senderOffset = 0;
                            senderDataLength = 0;

                            return result;
                        }

                        onSentAction?.Invoke(0);

                        return SendResults.Sent;
                    }

                    int length = 0;

                    for (int i = 0; i < dataArr.Length; i++)
                    {
                        var arr = dataArr[i];

                        for (int j = 0; j < Math.Ceiling((double)arr.Length / senderBuffer.Length); j++)
                        {
                            length = Math.Min(senderBuffer.Length, arr.Length - (senderBuffer.Length * j));

                            if (AddBuffer(arr, j * senderBuffer.Length, length))
                            {
                                continue;
                            }

                            if ((result = SendBuffer()) != SendResults.Sent)
                            {
                                senderOffset = 0;
                                senderDataLength = 0;

                                return result;
                            }

                            AddBuffer(arr, j * senderBuffer.Length, length);
                        }

                        if (senderDataLength > 0 && (result = SendBuffer()) != SendResults.Sent)
                        {
                            senderOffset = 0;
                            senderDataLength = 0;

                            return result;
                        }
                    }

                    onSentAction?.Invoke(0);

                    return SendResults.Sent;
                }
                finally
                {
                    senderCompletedSignalEvent.Set();
                    sendAutoResetEvent.Set();
                }
            }

            public void BeginEmergency()
            {
                senderCompletedSignalEvent.WaitOne();
                emergentCompletedSignalEvent.Reset();

                _emergencyFlag = true;
                OnEmergentStart?.Invoke();
            }

            public void EndEmergency()
            {
                _emergencyFlag = false;
                emergentCompletedSignalEvent.Set();
            }

            public async Task<bool> SendEmergentDataAsync(byte[][] dataArr, Action onSentAction = null)
            {
                //var length = dataArr.Sum(c => c.Length);

                //if (length > Socket.SendBufferSize)
                //{
                //    throw new Exception($"Emergent data length must be less or equal then socket send buffer size that is {Socket.SendBufferSize} bytes. But data length is {length} bytes");
                //}
                await Task.Yield();

                lock (emergencySendLckObj)
                {
                    if (!_emergencyFlag)
                    {
                        throw new Exception("Couldnt send emergent data when state not emergent");
                    }

                    var offset = 0;

                    for (int i = 0; i < dataArr.Length; i++)
                    {
                        var arr = dataArr[i];

                        Buffer.BlockCopy(arr, 0, emergentSenderBuffer, offset, arr.Length);

                        offset += arr.Length;
                    }

                    emergentWriteEventArgs.SetBuffer(0, offset);

                    if (Send(emergentWriteEventArgs, emergentSentSignalEvent) < offset)
                    {
                        return false;
                    }

                    onSentAction?.Invoke();

                    return true;
                }
            }

            public bool Poll()
            {
                return !Socket.Poll(1, SelectMode.SelectRead) && Socket.Available == 1;
            }

            private void Connect()
            {
                lock (connectLckObj)
                {
                    while (true)
                    {
                        while (Interlocked.Increment(ref currentRetryCount) <= Receiver.ConnectRetry)
                        {
                            try
                            {
                                if (Socket != null && Socket.Connected)
                                {
                                    //if (Poll())
                                    //{
                                    //    Interlocked.Exchange(ref currentRetryCount, 0);

                                    //    return;
                                    //}
                                    Socket.Shutdown(SocketShutdown.Both);
                                    Socket.Close();
                                }

                                Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
                                {
                                    ReceiveBufferSize = Receiver.ReceiverBufferSize,
                                    SendBufferSize = Receiver.SendBufferSize
                                };

                                //SetKeepAlive(500, 100);// 100 * 10 + 500 = 1.5 s

                                Socket.Connect(Receiver.EndPointList[selectedEndpointIndex]);

                                Interlocked.Exchange(ref currentRetryCount, 0);

                                connectSignalEvent.Set();

                                Console.WriteLine($"Socket connected. Endpoint => {Receiver.EndPointList[selectedEndpointIndex]}");
                                Receiver.OnConnected?.Invoke(this);

                                return;
                            }
                            catch (Exception ex)
                            {
                                Receiver.OnException?.Invoke(ex, this);
                                Thread.Sleep(Receiver.ConnectRetryInterval);

                                continue;
                            }
                        }

                        Interlocked.Exchange(ref currentRetryCount, 0);

                        Receiver.OnConnectionTryFailed?.Invoke(Receiver.EndPointList[selectedEndpointIndex], this);

                        selectedEndpointIndex = (selectedEndpointIndex + 1) % Receiver.EndPointList.Length;
                    }
                }
            }

            public void WaitWhileConnect()
            {
                connectSignalEvent.WaitOne();
            }

            public void Reset(bool changeConnection = true, bool waitToConnect = true)
            {
                connectSignalEvent.Reset();

                if (changeConnection)
                {
                    selectedEndpointIndex = (selectedEndpointIndex + 1) % Receiver.EndPointList.Length;
                }

                Socket.Shutdown(SocketShutdown.Both);

                Poll();

                if (waitToConnect)
                {
                    connectSignalEvent.WaitOne();
                }
            }

            public void Reset()
            {
                Reset(true, true);
            }

            public void Dispose()
            {
                if (!_disposed)
                {
                    Socket.Dispose();

                    receivedSignalEvent.Dispose();
                    sentSignalEvent.Dispose();
                    connectSignalEvent.Dispose();
                    senderCompletedSignalEvent.Dispose();

                    readEventArgs.Dispose();
                    writeEventArgs.Dispose();

                    _disposed = true;
                }

                GC.SuppressFinalize(this);
            }
        }
    }
}
