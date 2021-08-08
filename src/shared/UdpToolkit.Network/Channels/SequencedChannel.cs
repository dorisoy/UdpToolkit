namespace UdpToolkit.Network.Channels
{
    using UdpToolkit.Network.Contracts.Channels;

    public sealed class SequencedChannel : IChannel
    {
        public static readonly byte Id = ReliableChannelConsts.SequencedChannel;
        private ushort _lastReceivedNumber;
        private ushort _sequenceNumber;

        public SequencedChannel()
        {
            _sequenceNumber = 0;
        }

        public bool IsReliable { get; } = false;

        public byte ChannelId { get; } = Id;

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

        public bool IsDelivered(
            ushort id)
        {
            return true;
        }

        public void HandleOutputPacket(
            out ushort id,
            out uint acks)
        {
            id = ++_sequenceNumber;
            acks = default;
        }

        public bool HandleAck(
            ushort id,
            uint acks)
        {
            return true;
        }
    }
}