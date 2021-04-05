namespace UdpToolkit.Network.Channels
{
    using System;

    public sealed class SequencedChannel : IChannel
    {
        private readonly object _locker = new object();
        private ushort _lastReceivedNumber;
        private ushort _sequenceNumber;

        public SequencedChannel()
        {
            _sequenceNumber = 0;
        }

        public bool HandleInputPacket(
            ushort id,
            uint acks)
        {
            lock (_locker)
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
            lock (_locker)
            {
                id = ++_sequenceNumber;
                acks = default;
            }
        }

        public bool HandleAck(
            ushort id,
            uint acks)
        {
            return true;
        }
    }
}