#pragma warning disable
namespace UdpToolkit.Benchmarks
{
    using System;
    using System.Buffers;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;
    using System.Threading.Tasks;
    using BenchmarkDotNet.Attributes;
    using UdpToolkit.Benchmarks.Utils;

    [HtmlExporter]
    [MemoryDiagnoser]
    [ThreadingDiagnoser]
    public class UdpReceiveBenchmark
    {
        private static readonly ConcurrentPool<EventArgsWrapper> ArgPoolSending =
            new ConcurrentPool<EventArgsWrapper>(
                () => new EventArgsWrapper((a, b) =>
            {
            }), 10000);

        private static readonly ConcurrentPool<EventArgsWrapper> ArgPoolReceiving =
            new ConcurrentPool<EventArgsWrapper>(
                () => new EventArgsWrapper((a, b) =>
                {
                    Console.WriteLine(Thread.CurrentThread.ManagedThreadId);
                }), 10000);

        private static readonly IPEndPoint ServerIp = new IPEndPoint(IPAddress.Loopback, 5000);
        private static readonly IPEndPoint ClientIp = new IPEndPoint(IPAddress.Loopback, 10000);
        private static readonly IPEndPoint AnyIp = new IPEndPoint(IPAddress.Any, 0);
        private EndPoint AnyIp2 = new IPEndPoint(IPAddress.Any, 0);

        private int Counter = 0;
        private static readonly ManualResetEvent Wait = new ManualResetEvent(false);

        private static UdpClient _server;
        private Socket _clientV2;

        private readonly CancellationTokenSource cts = new CancellationTokenSource();
        private Thread t;

        [Params(100, 1000, 10000)]
        public int N;
        
        [GlobalSetup]
        public void Setup()
        {
            _server = new UdpClient();
            _server.Client.Blocking = false;
            _server.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            _server.Client.Bind(ServerIp);
            //
            // _client = new UdpClient();
            // _client.Client.Blocking = false;
            // _client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            // _client.Client.Bind(ClientIp);

            _clientV2 = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            _clientV2.Blocking = false;
            _clientV2.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            _clientV2.Bind(ClientIp);

            // _serverV2 = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            // _serverV2.Blocking = false;
            // _serverV2.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            // _serverV2.Bind(ServerIp);

            t = new Thread(() =>
            {
                while (!cts.IsCancellationRequested)
                {
                    using (var polledWrapper = ArgPoolSending.Get())
                    {
                        polledWrapper.Value.Set(ServerIp);
                        _clientV2.SendToAsync(polledWrapper.Value.SocketAsyncEventArgs);
                    }
                }
            });
            
            t.Start();
        }

        [Benchmark]
        public async Task ReceiveAsync_ReceiveAsync_Task()
        {
            _server.Client.Blocking = false;
            int counter = 0;
            while (true)
            {
                await _server.ReceiveAsync().ConfigureAwait(false);
                counter++;
                if (counter == N)
                {
                    Wait.Set();
                    break;
                }
            }
            

            Wait.WaitOne();
        }

        [Benchmark]
        public void ReceiveAsync_Receive_Sync()
        {
            _server.Client.Blocking = true;
            int counter = 0;
            while (true)
            {
                var ip = AnyIp;
                _server.Receive(ref ip);
                counter++;
                if (counter == N)
                {
                    Wait.Set();
                    break;
                }
            }
            

            Wait.WaitOne();
        }
        
        [Benchmark]
        public void ReceiveAsync_BeginReceive()
        {
            _server.Client.Blocking = false;
            _server.BeginReceive(Recv, null);
            Wait.WaitOne();
        }

        private void Recv(IAsyncResult res)
        {
            var ep = AnyIp;
            _server.EndReceive(res, ref ep);
            Counter++;

            if (Counter == N)
            {
                Wait.Set();
            }

            _server.BeginReceive(Recv, null);
        }

        [Benchmark]
        public void ReceiveAsync_ReceiveMessageFrom_Blocking()
        {
            _server.Client.Blocking = true;
            while (true)
            {
                var buffer = ArrayPool<byte>.Shared.Rent(10);
                _server.Client.ReceiveFrom(
                    buffer,
                    0,
                     10,
                    SocketFlags.None,
                    ref AnyIp2);

                Counter++;
                if (Counter == N)
                {
                    ArrayPool<byte>.Shared.Return(buffer);
                    break;
                }

                ArrayPool<byte>.Shared.Return(buffer);
            }

            Counter = 0;
        }

        [Benchmark]
        public async ValueTask ReceiveAsync_ReceiveMessageFrom_NonBlocking()
        {
            _server.Client.Blocking = false;
            Counter = 0;
            while (true)
            {
                var array = ArrayPool<byte>.Shared.Rent(100);
                await _server.Client.ReceiveFromAsync(array.AsMemory());
                ArrayPool<byte>.Shared.Return(array);
                Counter++;
                if (Counter == N)
                {
                    break;
                }
            }
        }

        [GlobalCleanup]
        public void CleanUp()
        {
            Counter = 0;
            cts.Cancel();
            t.Join();
        }
    }
}