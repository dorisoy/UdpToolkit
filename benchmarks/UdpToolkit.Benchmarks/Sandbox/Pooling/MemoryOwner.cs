namespace UdpToolkit.Benchmarks.Sandbox.Pooling
{
    using System;
    using System.Buffers;

    // Struct implements the interface so it can be boxed if necessary.
    public struct MemoryOwner<T> : IMemoryOwner<T>
    {
        private T[] _array;

        public MemoryOwner(int minBufferSize)
        {
            _array = NonAllocatingPool<T>.SharedPool.Rent(minBufferSize);
        }

        public Memory<T> Memory
        {
            get
            {
                if (_array == null)
                {
                    throw new ObjectDisposedException("Memory already in pool");
                }

                return new Memory<T>(_array);
            }
        }

        public void Dispose()
        {
            if (_array != null)
            {
                NonAllocatingPool<T>.SharedPool.Return(_array);
                _array = null;
            }
        }
    }
}