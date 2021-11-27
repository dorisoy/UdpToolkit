namespace UdpToolkit.Benchmarks
{
    using BenchmarkDotNet.Running;
    using UdpToolkit.Benchmarks.Sandbox.Pooling;
    using UdpToolkit.Benchmarks.Sandbox.Queues;
    using UdpToolkit.Benchmarks.Sandbox.Serialization;

    public static class Program
    {
        public static void Main()
        {
            BenchmarkRunner.Run<UdpClientBenchmark>();
            BenchmarkRunner.Run<ProtobufSerializationBenchmark>();
            BenchmarkRunner.Run<MessagePackSerializationBenchmark>();
            BenchmarkRunner.Run<NetProtobufSerializationBenchmark>();
            BenchmarkRunner.Run<MemoryPoolBenchmark>();
            BenchmarkRunner.Run<ArrayPoolBenchmark>();
            BenchmarkRunner.Run<SubscriptionBenchmark>();
            BenchmarkRunner.Run<UnsafeSerializationBenchmark>();
        }
    }
}