namespace UdpToolkit.Network.Contracts.Pooling
{
    using System;
    using System.Collections.Concurrent;

    /// <summary>
    /// Concurrent pool.
    /// </summary>
    /// <typeparam name="T">Type of pooled object.</typeparam>
    public sealed class ConcurrentPool<T>
        where T : IDisposable
    {
        private readonly Func<ConcurrentPool<T>, T> _factory;
        private readonly ConcurrentBag<T> _pool;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentPool{T}"/> class.
        /// </summary>
        /// <param name="factory">Instance factory.</param>
        /// <param name="initSize">Init size of pool.</param>
        public ConcurrentPool(
            Func<ConcurrentPool<T>, T> factory,
            int initSize)
        {
            _factory = factory;
            _pool = new ConcurrentBag<T>();

            for (int i = 0; i < initSize; i++)
            {
                var instance = _factory(this);
                _pool.Add(instance);
            }
        }

        /// <summary>
        /// Get objects from pool or create new instance.
        /// </summary>
        /// <returns>Instance of object.</returns>
        public T GetOrCreate()
        {
            if (_pool.TryTake(out var returnObject))
            {
                return returnObject;
            }

            return _factory(this);
        }

        /// <summary>
        /// Return object to pool.
        /// </summary>
        /// <param name="instance">Instance of pooled object.</param>
        public void Return(T instance)
        {
            if (instance != null)
            {
                _pool.Add(instance);
            }
        }
    }
}