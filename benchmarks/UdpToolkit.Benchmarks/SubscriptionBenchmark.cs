namespace UdpToolkit.Benchmarks
{
    using System;
    using BenchmarkDotNet.Attributes;
    using UdpToolkit.Framework.Contracts;
    using UdpToolkit.Network.Contracts.Sockets;

    [HtmlExporter]
    [MemoryDiagnoser]
    [ThreadingDiagnoser]
    public class SubscriptionBenchmark
    {
#pragma warning disable SA1401

        [Params(100, 1000, 10000)]
        public int Repeats;
#pragma warning restore SA1401

        private static readonly Ping CachedPing = new Ping();
        private readonly Subscription<Ping> _subscription;

        public SubscriptionBenchmark()
        {
            _subscription = new Subscription<Ping>(
                onEvent: (connectionId, ip, ping) => { return Guid.Empty; },
                onTimeout: () => { });
        }

        [Benchmark]
        public void OnEvent()
        {
            for (int i = 0; i < Repeats; i++)
            {
                _subscription.OnEvent(Guid.NewGuid(), new IpV4Address(0, 0), CachedPing);
            }
        }

        [Benchmark]
        public void OnTimeout()
        {
            for (int i = 0; i < Repeats; i++)
            {
                _subscription.OnTimeout();
            }
        }

        private class Ping
        {
        }
    }
}