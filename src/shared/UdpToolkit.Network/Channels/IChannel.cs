namespace UdpToolkit.Network.Channels
{
    using System;
    using System.Collections.Generic;
    using UdpToolkit.Network.Packets;
    using UdpToolkit.Network.Pooling;

    public interface IChannel
    {
        bool HandleInputPacket(
            NetworkPacket networkPacket);

        void HandleOutputPacket(
            NetworkPacket networkPacket);

        bool HandleAck(
            NetworkPacket networkPacket);

        void GetAck(
            NetworkPacket networkPacket);

        bool IsDelivered(
            ushort networkPacketId);
    }
}