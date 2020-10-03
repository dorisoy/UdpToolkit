namespace UdpToolkit.Network.Channels
{
    using System.Collections.Generic;
    using UdpToolkit.Network.Packets;
    using UdpToolkit.Network.Queues;

    public interface IChannel
    {
        ChannelResult TryHandleInputPacket(
            NetworkPacket networkPacket);

        NetworkPacket TryHandleOutputPacket(
            NetworkPacket networkPacket);

        NetworkPacket HandleAck(
            NetworkPacket networkPacket);

        IEnumerable<NetworkPacket> GetPendingPackets();

        IEnumerable<NetworkPacket> ToResend();

        void Flush();
    }
}