namespace UdpToolkit.Framework
{
    using System;
    using UdpToolkit.Framework.Contracts;
    using UdpToolkit.Network.Contracts.Packets;

    public interface IBroadcaster : IDisposable
    {
        void Broadcast(
            Func<byte[]> serializer,
            Guid caller,
            int roomId,
            byte hookId,
            PacketType packetType,
            byte channelId,
            BroadcastMode broadcastMode);
    }
}