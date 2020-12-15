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
                Console.WriteLine($"RETRIEVED FROM POOL {typeof(T).Name}");
                return returnObject;
            }

            Console.WriteLine($"CREATED NEW OBJECT {typeof(T).Name}");
            return new PooledObject<T>(_factory(), this);
        }

        public void Return(PooledObject<T> obj)
        {
            if (obj != null)
            {
                Console.WriteLine($"RETURNED TO POOL {typeof(T).Name}");
                _pool.Add(obj);
            }
            else
            {
                Console.WriteLine($"POOLED OBJECT {typeof(T).Name} IS NULL!!!");
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