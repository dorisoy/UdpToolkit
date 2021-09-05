namespace UdpToolkit.Network.Channels
{
    using UdpToolkit.Network.Contracts.Channels;

    /// <summary>
    /// Reliable UDP channel.
    /// </summary>
    public sealed class ReliableChannel : IChannel
    {
        /// <summary>
        /// Reserved chanel identifier.
        /// </summary>
        public static readonly byte Id = ReliableChannelConsts.ReliableChannel;

        private readonly NetWindow _netWindow;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReliableChannel"/> class.
        /// </summary>
        /// <param name="windowSize">Network window size.</param>
        public ReliableChannel(
            int windowSize)
        {
            _netWindow = new NetWindow(windowSize);
        }

        /// <inheritdoc />
        public bool IsReliable { get; } = true;

        /// <inheritdoc />
        public byte ChannelId { get; } = Id;

        /// <inheritdoc />
        public bool HandleInputPacket(
            ushort id,
            uint acks)
        {
            if (!_netWindow.CanSet(id))
            {
                return false;
            }

            _netWindow.InsertPacketData(
                id: id,
                acks: acks,
                acked: true);

            return true;
        }

        /// <inheritdoc />
        public bool IsDelivered(
            ushort id)
        {
            return _netWindow.IsDelivered(id);
        }

        /// <inheritdoc />
        public void HandleOutputPacket(
            out ushort id,
            out uint acks)
        {
            id = _netWindow.Next();
            acks = FillAcks();

            _netWindow.InsertPacketData(
                id: id,
                acks: acks,
                acked: false);
        }

        /// <inheritdoc />
        public bool HandleAck(
            ushort id,
            uint acks)
        {
            if (!_netWindow.IsDelivered(id))
            {
                return _netWindow.TryAcceptAck(
                    id: id,
                    acks: acks);
            }

            return false;
        }

#pragma warning disable S3400
        private uint FillAcks()
#pragma warning restore S3400
        {
            // not supported right now
            return 0;
        }
    }
}