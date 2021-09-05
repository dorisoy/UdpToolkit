namespace UdpToolkit.Network.Channels
{
    using System;
    using UdpToolkit.Network.Contracts.Channels;

    /// <summary>
    /// Reliable ordered UDP channel.
    /// </summary>
    public sealed class ReliableOrderedChannel : IChannel
    {
        /// <summary>
        /// Reserved chanel identifier.
        /// </summary>
        public static readonly byte Id = ReliableChannelConsts.ReliableOrderedChannel;

        /// <inheritdoc />
        public bool IsReliable { get; } = true;

        /// <inheritdoc />
        public byte ChannelId { get; } = Id;

        /// <inheritdoc />
        public bool HandleInputPacket(
            ushort id,
            uint acks)
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc />
        public bool IsDelivered(
            ushort id)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void HandleOutputPacket(
            out ushort id,
            out uint acks)
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc />
        public bool HandleAck(
            ushort id,
            uint acks)
        {
            throw new System.NotImplementedException();
        }
    }
}