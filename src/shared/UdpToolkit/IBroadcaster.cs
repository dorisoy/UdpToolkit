namespace UdpToolkit
{
    using System;
    using UdpToolkit.Core;
    using UdpToolkit.Network;
    using UdpToolkit.Network.Channels;

    public interface IBroadcaster : IDisposable
    {
        void Broadcast(
            Func<byte[]> serializer,
            Guid caller,
            int roomId,
            byte hookId,
            NetworkPacketType networkPacketType,
            ChannelType channelType,
            BroadcastMode broadcastMode);

        void Broadcast(
            Func<byte[]> serializer,
            Guid caller,
            byte hookId,
            NetworkPacketType networkPacketType,
            ChannelType channelType,
            BroadcastMode broadcastMode);
    }
}