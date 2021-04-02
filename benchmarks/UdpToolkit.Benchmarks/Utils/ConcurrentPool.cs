namespace UdpToolkit.Benchmarks.Utils
{
    using System;
    using System.Collections.Concurrent;

    public sealed class ConcurrentPool<T>
        where T : IResettable
    {
        private readonly ConcurrentBag<PooledObject<T>> _pool;
        private readonly Func<T> _factory;

        public ConcurrentPool(Func<T> factory, int initValue)
        {
            _factory = factory;
            _pool = new ConcurrentBag<PooledObject<T>>();
            for (int i = 0; i < initValue; i++)
            {
                var newObject = _factory();
                _pool.Add(new PooledObject<T>(this, newObject));
            }
        }

        public int Count => _pool.Count;

        public PooledObject<T> Get()
        {
            if (_pool.TryTake(out PooledObject<T> returnObject))
            {
                return returnObject;
            }

            Console.WriteLine("NEW");
            T newObject = _factory();
            return new PooledObject<T>(this, newObject);
        }

        public void Return(PooledObject<T> inToReturn)
        {
            if (inToReturn != null)
            {
                _pool.Add(inToReturn);
            }
        }
    }
}