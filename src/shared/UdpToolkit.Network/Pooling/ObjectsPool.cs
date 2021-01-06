namespace UdpToolkit.Network.Pooling
{
    using System;
    using System.Collections.Concurrent;

    public sealed class ObjectsPool<T> : IObjectsPool<T>
        where T : IResetteble
    {
        private readonly ConcurrentBag<PooledObject<T>> _pool = new ConcurrentBag<PooledObject<T>>();
        private readonly Func<T> _factory;

        public ObjectsPool(
            Func<T> inFactory,
            int startSize)
        {
            _factory = inFactory;
            WarmUp(startSize: startSize);
        }

        public PooledObject<T> Get()
        {
            if (_pool.TryTake(out var returnObject))
            {
                return returnObject;
            }

            return new PooledObject<T>(_factory(), this);
        }

        public void Return(PooledObject<T> obj)
        {
            if (obj != null)
            {
                _pool.Add(obj);
            }
        }

        private void WarmUp(int startSize)
        {
            for (var i = 0; i < startSize; i++)
            {
                _pool.Add(new PooledObject<T>(_factory(), this));
            }
        }
    }
}