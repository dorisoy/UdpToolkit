namespace UdpToolkit.Network.Channels
{
    using System;
    using System.Collections.Generic;
    using UdpToolkit.Network.Packets;

    public interface IChannel
    {
        bool HandleInputPacket(
            NetworkPacket networkPacket);

        void HandleOutputPacket(
            NetworkPacket networkPacket);

        void GetNext(
            NetworkPacket networkPacket);

        bool HandleAck(
            NetworkPacket networkPacket);

        NetworkPacket GetAck(
            NetworkPacket networkPacket);

        IEnumerable<NetworkPacket> ToResend(TimeSpan resendTimeout);
    }
}