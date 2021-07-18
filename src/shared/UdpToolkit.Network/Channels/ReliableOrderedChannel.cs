namespace UdpToolkit.Network.Channels
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    [ExcludeFromCodeCoverage]
    public sealed class ReliableOrderedChannel : IChannel
    {
        public bool HandleInputPacket(
            ushort id,
            uint acks)
        {
            throw new System.NotImplementedException();
        }

        public bool IsDelivered(
            ushort id)
        {
            throw new NotImplementedException();
        }

        public void HandleOutputPacket(
            out ushort id,
            out uint acks)
        {
            throw new System.NotImplementedException();
        }

        public bool HandleAck(
            ushort id,
            uint acks)
        {
            throw new System.NotImplementedException();
        }
    }
}