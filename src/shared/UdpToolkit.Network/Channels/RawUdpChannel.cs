namespace UdpToolkit.Network.Channels
{
    using UdpToolkit.Network.Contracts.Channels;
    using UdpToolkit.Network.Contracts.Protocol;

    /// <summary>
    /// Raw UDP channel.
    /// </summary>
    public sealed class RawUdpChannel : IChannel
    {
        /// <summary>
        /// Reserved chanel identifier.
        /// </summary>
        public static readonly byte Id = ReliableChannelConsts.RawChannel;

        /// <summary>
        /// Initializes a new instance of the <see cref="RawUdpChannel"/> class.
        /// </summary>
        public RawUdpChannel()
        {
        }

        /// <inheritdoc />
        public bool IsReliable { get; } = false;

        /// <inheritdoc />
        public byte ChannelId { get; } = Id;

        /// <inheritdoc />
        public bool HandleInputPacket(
            in NetworkHeader networkHeader)
        {
            return true;
        }

        /// <inheritdoc />
        public ushort HandleOutputPacket(
            byte dataType)
        {
            return 0;
        }

        /// <inheritdoc />
        public bool HandleAck(
            in NetworkHeader networkHeader)
        {
            return true;
        }

        /// <inheritdoc />
        public bool IsDelivered(ushort id)
        {
            return true;
        }
    }
}