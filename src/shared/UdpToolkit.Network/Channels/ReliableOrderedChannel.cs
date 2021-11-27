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

        /// <summary>
        /// Initializes a new instance of the <see cref="ReliableOrderedChannel"/> class.
        /// </summary>
        public ReliableOrderedChannel()
        {
        }

        /// <inheritdoc />
        public bool IsReliable { get; } = true;

        /// <inheritdoc />
        public byte ChannelId { get; } = Id;

        /// <inheritdoc />
        public bool HandleInputPacket(
            in NetworkHeader networkHeader)
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc />
        public ushort HandleOutputPacket(
            byte dataType)
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc />
        public bool HandleAck(
            in NetworkHeader networkHeader)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public bool IsDelivered(ushort id)
        {
            throw new NotImplementedException();
        }
    }
}