namespace UdpToolkit.Benchmarks
{
    using System.Buffers;
    using System.Net;
    using System.Net.Sockets;
    using BenchmarkDotNet.Attributes;
    using UdpToolkit.Benchmarks.Utils;

    [HtmlExporter]
    [MemoryDiagnoser]
    [ThreadingDiagnoser]
    public class UdpSendBenchmark
    {
#pragma warning disable
        [Params(1, 10, 100, 1000)]
        public int Jobs;
#pragma warning restore

        private static readonly IPEndPoint _ip = new IPEndPoint(IPAddress.Loopback, 8080);

        private static readonly ConcurrentPool<EventArgsWrapper> _argPool =
            new ConcurrentPool<EventArgsWrapper>(() => new EventArgsWrapper((a, b) => { }), 0);

        private UdpClient _udpClient;

        [GlobalSetup]
        public void SetUp()
        {
            _udpClient = new UdpClient();
        }

        [Benchmark]
        public void Send()
        {
            for (var i = 0; i < Jobs; i++)
            {
                _udpClient.Send(new byte[10] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, 10, _ip);
            }
        }

        [Benchmark]
        public void Send_Pooled()
        {
            for (int j = 0; j < Jobs; j++)
            {
                var array = ArrayPool<byte>.Shared.Rent(10);
                for (byte i = 0; i < 10; i++)
                {
                    array[i] = i;
                }

                _udpClient.Send(new byte[10], 10, _ip);
                ArrayPool<byte>.Shared.Return(array);
            }
        }

        [Benchmark]
        public void Send_Client_Async()
        {
            for (int i = 0; i < Jobs; i++)
            {
                using (var e = new SocketAsyncEventArgs())
                {
                    e.SocketFlags = SocketFlags.None;
                    e.RemoteEndPoint = _ip;
                    e.Completed += (a, b) => { };
                    e.SetBuffer(new byte[10] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 });
                    _udpClient.Client.SendAsync(e);
                }
            }
        }

        [Benchmark]
        public void Send_Client_Async_Pooled()
        {
            for (int i = 0; i < Jobs; i++)
            {
                using (var polledWrapper = _argPool.Get())
                {
                    polledWrapper.Value.Set(_ip);
                    _udpClient.Client.SendAsync(polledWrapper.Value.SocketAsyncEventArgs);
                }
            }
        }

        [Benchmark]
        public void Send_Client_Async_Pooled_With_Polled_Buffer()
        {
            for (int i = 0; i < Jobs; i++)
            {
                using (var polledWrapper = _argPool.Get())
                {
                    polledWrapper.Value.Set(_ip);
                    _udpClient.Client.SendAsync(polledWrapper.Value.SocketAsyncEventArgs);
                }
            }
        }

        [Benchmark]
        public void Send_Client_Async_Pooled_With_Polled_Buffer_Initialized()
        {
            for (int i = 0; i < Jobs; i++)
            {
                using (var polledWrapper = _argPool.Get())
                {
                    polledWrapper.Value.Set(_ip);
                    _udpClient.Client.SendAsync(polledWrapper.Value.SocketAsyncEventArgs);
                }
            }
        }
    }
}