namespace UdpToolkit.Network.Pooling
{
    using System;

    public class PooledObject<T> : IDisposable
        where T : IResetteble
    {
        private readonly IObjectsPool<T> _objectsPool;
        private readonly T _obj;

        public PooledObject(
            T obj,
            IObjectsPool<T> objectsPool)
        {
            _objectsPool = objectsPool;
            _obj = obj;
        }

        public T Value => _obj;

        public void Dispose()
        {
            _obj.Reset();
            _objectsPool.Return(this);
        }
    }
}