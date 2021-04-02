namespace UdpToolkit.Benchmarks
{
    using BenchmarkDotNet.Running;

    public static class Program
    {
        public static void Main(string[] args)
        {
#pragma warning disable
            // BenchmarkRunner.Run<ProducerConsumerQueueBenchmark>();
            // BenchmarkRunner.Run<UdpReceiveBenchmark>();
            BenchmarkRunner.Run<DynamicBenchmark>();
            // var bench = new UdpReceiveBenchmark();
            // bench.Setup();
            // bench.ReceiveAsync_ReceiveMessageFrom_Blocking();

            // BenchmarkRunner.Run<GenericSerializationBenchmark>();
            // BenchmarkRunner.Run<ProtocolSerializationBenchmark>();
        }
    }
}
