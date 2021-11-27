namespace UdpToolkit.Benchmarks.Sandbox.Pooling
{
    using System.Buffers;
    using BenchmarkDotNet.Attributes;
    using UdpToolkit.Network.Contracts.Pooling;

    [HtmlExporter]
    [MemoryDiagnoser]
    [ThreadingDiagnoser]
    public class ArrayPoolBenchmark
    {
#pragma warning disable SA1401
        [Params(100, 1000, 5000, 10000)]
        public int Repeats;
#pragma warning restore SA1401
        private const int ArraySize = 2048;
        private ConcurrentArrayPool _pool;

        [IterationSetup(Target = nameof(CustomArrayPoolBench))]
        public void Setup()
        {
            _pool = new ConcurrentArrayPool(1, ArraySize);
        }

        [Benchmark]
        public void ArrayPoolBench()
        {
            for (int i = 0; i < Repeats; i++)
            {
                var pooledArray = ArrayPool<byte>.Shared.Rent(ArraySize);
                ArrayPool<byte>.Shared.Return(pooledArray);
            }
        }

        [Benchmark]
        public void CustomArrayPoolBench()
        {
            for (int i = 0; i < Repeats; i++)
            {
                var pooledArray = _pool.GetOrCreate();
                _pool.Return(pooledArray);
            }
        }
    }
}