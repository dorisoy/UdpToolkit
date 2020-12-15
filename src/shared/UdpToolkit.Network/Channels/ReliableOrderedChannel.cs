namespace UdpToolkit.Network.Channels
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using UdpToolkit.Network.Packets;

    [ExcludeFromCodeCoverage]
    public sealed class ReliableOrderedChannel : IChannel
    {
        public bool HandleInputPacket(
            NetworkPacket networkPacket)
        {
            throw new System.NotImplementedException();
        }

        public void GetAck(
            NetworkPacket networkPacket)
        {
        }

        public bool IsDelivered(
            ushort networkPacketId)
        {
            throw new NotImplementedException();
        }

        public void HandleOutputPacket(
            NetworkPacket networkPacket)
        {
        }

        public bool HandleAck(
            NetworkPacket networkPacket)
        {
            throw new System.NotImplementedException();
        }
    }
}