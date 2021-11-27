namespace UdpToolkit.Benchmarks.Sandbox.Pooling
{
    using System.Buffers;
    using System.Collections.Generic;
    using BenchmarkDotNet.Attributes;
    using UdpToolkit.Network.Contracts.Pooling;

    [HtmlExporter]
    [MemoryDiagnoser]
    [ThreadingDiagnoser]
    public class MemoryPoolBenchmark
    {
#pragma warning disable SA1401
        [Params(100, 1000, 10_000)]
        public int Repeats;
#pragma warning restore SA1401

        private const int ArraySize = 1500;

        private List<byte[]> _polledByteArrays;
        private List<IMemoryOwner<byte>> _iMemoryOwners;
        private List<MemoryOwner<byte>> _memoryOwners;

        private ConcurrentArrayPool _concurrentArrayPool;
        private ArrayPool<byte> _configuredArrayPool;
        private List<byte[]> _list;

        [IterationSetup(Target = nameof(ConfiguredArrayPool))]
        public void SetupConfiguredArrayPool()
        {
            var listCapacity = Repeats * 2;
            var list = new List<byte[]>(listCapacity);
            _configuredArrayPool = ArrayPool<byte>.Create(2048, 15000);
            for (var i = 0; i < Repeats; i++)
            {
                var bytes = _configuredArrayPool.Rent(ArraySize);
                list.Add(bytes);
            }

            for (var i = 0; i < Repeats; i++)
            {
                var bytes = list[i];
                _configuredArrayPool.Return(bytes);
            }

            _polledByteArrays = new List<byte[]>(listCapacity);
        }

        [IterationSetup(Target = nameof(MemoryPool))]
        public void SetupMemoryPool()
        {
            var listCapacity = Repeats * 2;
            var list = new List<IMemoryOwner<byte>>(listCapacity);
            for (var i = 0; i < Repeats; i++)
            {
                var memoryOwner = MemoryPool<byte>.Shared.Rent(ArraySize);
                list.Add(memoryOwner);
            }

            for (var i = 0; i < Repeats; i++)
            {
                var owner = list[i];
                owner.Dispose();
            }

            _iMemoryOwners = new List<IMemoryOwner<byte>>(listCapacity);
        }

        [IterationSetup(Target = nameof(NonAllocatingPool))]
        public void SetupNonAllocatingPool()
        {
            var listCapacity = Repeats * 2;
            var list = new List<byte[]>(listCapacity);
            for (var i = 0; i < Repeats; i++)
            {
                var polledArray = NonAllocatingPool<byte>.SharedPool.Rent(ArraySize);
                list.Add(polledArray);
            }

            for (var i = 0; i < Repeats; i++)
            {
                var polledArray = list[i];
                NonAllocatingPool<byte>.SharedPool.Return(polledArray);
            }

            _memoryOwners = new List<MemoryOwner<byte>>(listCapacity);
        }

        [IterationSetup(Target = nameof(ConcurrentArrayPool))]
        public void SetupConcurrentArrayPool()
        {
            var initSize = Repeats * 2;
            _concurrentArrayPool = new ConcurrentArrayPool(initSize, 2048);
            _list = new List<byte[]>(initSize);
        }

        [Benchmark]
        public void MemoryPool()
        {
            for (int i = 0; i < Repeats; i++)
            {
                var memoryOwner = MemoryPool<byte>.Shared.Rent(ArraySize);
                _iMemoryOwners.Add(memoryOwner);
            }

            for (int i = 0; i < Repeats; i++)
            {
                var item = _iMemoryOwners[i];
                item.Dispose();
            }
        }

        [Benchmark]
        public void ConfiguredArrayPool()
        {
            for (int i = 0; i < Repeats; i++)
            {
                var array = _configuredArrayPool.Rent(ArraySize);
                _polledByteArrays.Add(array);
            }

            for (int i = 0; i < Repeats; i++)
            {
                var array = _polledByteArrays[i];
                _configuredArrayPool.Return(array);
            }
        }

        [Benchmark]
        public void NonAllocatingPool()
        {
            for (int i = 0; i < Repeats; i++)
            {
                var memoryOwner = NonAllocatingPool<byte>.Shared.Rent(ArraySize);
                _memoryOwners.Add(memoryOwner);
            }

            for (int i = 0; i < Repeats; i++)
            {
                var element = _memoryOwners[i];
                element.Dispose();
            }
        }

        [Benchmark]
        public void ConcurrentArrayPool()
        {
            for (int i = 0; i < Repeats; i++)
            {
                var array = _concurrentArrayPool.GetOrCreate();
                _list.Add(array);
            }

            for (int i = 0; i < Repeats; i++)
            {
                var array = _list[i];
                _concurrentArrayPool.Return(array);
            }
        }
    }
}