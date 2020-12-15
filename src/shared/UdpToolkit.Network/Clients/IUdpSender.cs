namespace UdpToolkit.Network.Clients
{
    using System;
    using System.Threading.Tasks;
    using UdpToolkit.Network.Packets;
    using UdpToolkit.Network.Peers;
    using UdpToolkit.Network.Pooling;

    public interface IUdpSender : IDisposable
    {
        Task SendAsync(
            PooledObject<NetworkPacket> pooledNetworkPacket);

        Task SendAsync(
            PooledObject<NetworkPacket> pooledNetworkPacket,
            IRawPeer rawPeer,
            BroadcastType broadcastType);

        Task SendAsync(
            int roomId,
            PooledObject<NetworkPacket> pooledNetworkPacket,
            BroadcastType broadcastType);
    }
}