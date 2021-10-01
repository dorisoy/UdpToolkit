namespace UdpToolkit.Network.Channels
{
    using System;
    using UdpToolkit.Network.Contracts.Channels;
    using UdpToolkit.Network.Contracts.Protocol;

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
        public bool ResendOnHeartbeat { get; } = true;

        /// <inheritdoc />
        public byte ChannelId { get; } = Id;

        /// <inheritdoc />
        public bool HandleInputPacket(
            in NetworkHeader networkHeader)
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc />
        public bool IsDelivered(
            in NetworkHeader networkHeader)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public NetworkHeader HandleOutputPacket(
            byte dataType,
            Guid connectionId,
            PacketType packetType)
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc />
        public bool HandleAck(
            in NetworkHeader networkHeader)
        {
            throw new System.NotImplementedException();
        }
    }
}