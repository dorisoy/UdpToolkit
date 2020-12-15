namespace UdpToolkit.Network.Clients
{
    using System;
    using System.Threading.Tasks;
    using UdpToolkit.Network.Packets;
    using UdpToolkit.Network.Pooling;

    public interface IUdpReceiver : IDisposable
    {
        Task<PooledObject<NetworkPacket>> ReceiveAsync();
    }
}
