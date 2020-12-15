namespace UdpToolkit.Network.Channels
{
    using UdpToolkit.Network.Packets;

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
            NetworkPacket networkPacket)
        {
            lock (_locker)
            {
                var packetId = networkPacket.Id;
                var flag = packetId != _lastReceivedNumber;
                if (NetworkUtils.SequenceGreaterThan(packetId, _sequenceNumber) && flag)
                {
                    _lastReceivedNumber = packetId;
                    return true;
                }

                return false;
            }
        }

        public void GetAck(
            NetworkPacket networkPacket)
        {
        }

        public bool IsDelivered(ushort networkPacketId)
        {
            return true;
        }

        public void HandleOutputPacket(
            NetworkPacket networkPacket)
        {
            lock (_locker)
            {
                networkPacket.Set(
                    id: ++_sequenceNumber,
                    acks: 0);
            }
        }

        public bool HandleAck(
            NetworkPacket networkPacket)
        {
            return true;
        }
    }
}