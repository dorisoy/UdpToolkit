namespace UdpToolkit.Network.Clients
{
    using System;
    using System.Threading.Tasks;
    using UdpToolkit.Network.Packets;
    using UdpToolkit.Network.Pooling;

    public interface IUdpSender : IDisposable
    {
        Task SendAsync(
            PooledObject<NetworkPacket> pooledNetworkPacket);

        Task SendAsync(
            PooledObject<NetworkPacket> pooledNetworkPacket,
            IConnection connection,
            BroadcastType broadcastType);

        Task SendAsync(
            int roomId,
            PooledObject<NetworkPacket> pooledNetworkPacket,
            BroadcastType broadcastType);
    }
}