namespace UdpToolkit.Network.Pooling
{
    public interface IObjectsPool<T>
        where T : IResetteble
    {
        PooledObject<T> Get();

        void Return(PooledObject<T> obj);
    }
}