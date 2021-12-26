namespace UdpToolkit.Network.Channels
{
    using UdpToolkit.Network.Contracts.Channels;
    using UdpToolkit.Network.Contracts.Protocol;
    using UdpToolkit.Network.Utils;

    /// <summary>
    /// Sequenced channel.
    /// </summary>
    public sealed class SequencedChannel : IChannel
    {
        /// <summary>
        /// Reserved chanel identifier.
        /// </summary>
        public static readonly byte Id = ReliableChannelConsts.SequencedChannel;
        private readonly ushort[] _sequences;

        /// <summary>
        /// Initializes a new instance of the <see cref="SequencedChannel"/> class.
        /// </summary>
        /// <param name="sequences">Buffer for counters of sequences.</param>
        public SequencedChannel(ushort[] sequences)
        {
            _sequences = sequences;
        }

        /// <inheritdoc />
        public bool IsReliable { get; } = false;

        /// <inheritdoc />
        public byte ChannelId { get; } = Id;

        /// <inheritdoc />
        public bool HandleInputPacket(
            in NetworkHeader networkHeader)
        {
            var sequenceNumber = _sequences[networkHeader.DataType];
            if (NetworkUtils.SequenceGreaterThan(networkHeader.Id, sequenceNumber))
            {
                _sequences[networkHeader.DataType] = networkHeader.Id;
                return true;
            }

            return false;
        }

        /// <inheritdoc />
        public ushort HandleOutputPacket(
            byte dataType)
        {
            return ++_sequences[dataType];
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