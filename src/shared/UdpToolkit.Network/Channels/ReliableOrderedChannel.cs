namespace UdpToolkit.Network.Channels
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using UdpToolkit.Network.Contracts.Channels;

    [ExcludeFromCodeCoverage]
    public sealed class ReliableOrderedChannel : IChannel
    {
        public bool IsReliable { get; } = true;

        public byte ChannelId { get; } = ReliableChannelConsts.ReliableOrderedChannel;

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