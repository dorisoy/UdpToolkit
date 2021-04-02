namespace UdpToolkit.Benchmarks.Utils
{
    using System;

    public sealed class PooledObject<T> : IDisposable
        where T : IResettable
    {
        private readonly ConcurrentPool<T> _concurrentPool;
        private readonly T _pooledObject;

        public PooledObject(
            ConcurrentPool<T> concurrentPool,
            T pooledObject)
        {
            _concurrentPool = concurrentPool;
            _pooledObject = pooledObject;
        }

        public T Value => _pooledObject;

        public void Dispose()
        {
            _pooledObject.Reset();
            _concurrentPool.Return(this);
        }
    }
}