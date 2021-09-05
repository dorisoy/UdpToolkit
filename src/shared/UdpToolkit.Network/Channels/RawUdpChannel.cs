namespace UdpToolkit.Network.Channels
{
    using UdpToolkit.Network.Contracts.Channels;

    /// <summary>
    /// Raw UDP channel.
    /// </summary>
    public sealed class RawUdpChannel : IChannel
    {
        /// <summary>
        /// Reserved chanel identifier.
        /// </summary>
        public static readonly byte Id = ReliableChannelConsts.RawChannel;

        /// <inheritdoc />
        public bool IsReliable { get; } = false;

        /// <inheritdoc />
        public byte ChannelId { get; } = Id;

        /// <inheritdoc />
        public bool HandleInputPacket(
            ushort id,
            uint acks)
        {
            return true;
        }

        /// <inheritdoc />
        public bool IsDelivered(
            ushort id)
        {
            return true;
        }

        /// <inheritdoc />
        public void HandleOutputPacket(
            out ushort id,
            out uint acks)
        {
            id = default;
            acks = default;
        }

        /// <inheritdoc />
        public bool HandleAck(
            ushort id,
            uint acks)
        {
            return true;
        }
    }
}