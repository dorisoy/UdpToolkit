namespace UdpToolkit.Network.Channels
{
    using UdpToolkit.Network.Contracts.Channels;

    /// <summary>
    /// Sequenced channel.
    /// </summary>
    public sealed class SequencedChannel : IChannel
    {
        /// <summary>
        /// Reserved chanel identifier.
        /// </summary>
        public static readonly byte Id = ReliableChannelConsts.SequencedChannel;
        private ushort _lastReceivedNumber;
        private ushort _sequenceNumber;

        /// <summary>
        /// Initializes a new instance of the <see cref="SequencedChannel"/> class.
        /// </summary>
        public SequencedChannel()
        {
            _sequenceNumber = 0;
        }

        /// <inheritdoc />
        public bool IsReliable { get; } = false;

        /// <inheritdoc />
        public byte ChannelId { get; } = Id;

        /// <inheritdoc />
        public bool HandleInputPacket(
            ushort id,
            uint acks)
        {
            var flag = id != _lastReceivedNumber;
            if (NetworkUtils.SequenceGreaterThan(id, _sequenceNumber) && flag)
            {
                _lastReceivedNumber = id;
                _sequenceNumber = _lastReceivedNumber;
                return true;
            }

            return false;
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
            id = ++_sequenceNumber;
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