namespace UdpToolkit.Benchmarks.Sandbox.Pooling
{
    using System.Collections.Concurrent;

    /// <summary>
    /// Custom array pool.
    /// </summary>
    public sealed class ConcurrentArrayPool
    {
        private readonly int _size;
        private readonly ConcurrentBag<byte[]> _pool = new ConcurrentBag<byte[]>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentArrayPool"/> class.
        /// </summary>
        /// <param name="init">Init size of pool.</param>
        /// <param name="size">Size of buffer.</param>
        public ConcurrentArrayPool(int init, int size)
        {
            _size = size;
            for (int i = 0; i < init; i++)
            {
                _pool.Add(new byte[size]);
            }
        }

        /// <summary>
        /// Get array from pool or create new instance.
        /// </summary>
        /// <returns>Instance of object.</returns>
        public byte[] GetOrCreate()
        {
            if (_pool.TryTake(out var returnObject))
            {
                return returnObject;
            }

            return new byte[_size];
        }

        /// <summary>
        /// Return array to pool.
        /// </summary>
        /// <param name="instance">Instance of pooled object.</param>
        /// <param name="clearValues">Flag for clear data in array.</param>
        public void Return(byte[] instance, bool clearValues = false)
        {
            if (instance != null)
            {
                if (clearValues)
                {
                    for (int i = 0; i < instance.Length; i++)
                    {
                        instance[i] = default;
                    }
                }

                _pool.Add(instance);
            }
        }
    }
}