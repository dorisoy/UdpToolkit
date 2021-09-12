namespace UdpToolkit.Benchmarks
{
    using System;
    using BenchmarkDotNet.Running;

    public static class Program
    {
        public static void Main(
            string[] args)
        {
            BenchmarkRunner.Run<SubscriptionBenchmark>();
        }
    }
}