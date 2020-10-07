namespace UdpToolkit.Network.Channels
{
    using System.Collections.Generic;
    using System.Net;
    using UdpToolkit.Network.Packets;
    using UdpToolkit.Network.Queues;

    public interface IChannel
    {
        bool HandleInputPacket(
            NetworkPacket networkPacket);

        void HandleOutputPacket(
            NetworkPacket networkPacket);

        bool HandleAck(
            NetworkPacket networkPacket);

        NetworkPacket GetAck(
            NetworkPacket networkPacket,
            IPEndPoint ipEndPoint);

        IEnumerable<NetworkPacket> ToResend();
    }
}