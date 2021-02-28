namespace UdpToolkit.Network.Channels
{
    public sealed class NetWindow
    {
        private readonly int _windowSize;
        private readonly ushort?[] _ids;
        private readonly PacketData?[] _networkPackets;

        private ushort _maxId;

        public NetWindow(int windowSize)
        {
            _windowSize = windowSize;
            _ids = new ushort?[windowSize];
            _networkPackets = new PacketData?[windowSize];
        }

        public int Size => _windowSize;

        public ushort Next() => ++_maxId;

        public bool CanSet(ushort id)
        {
            if (id <= _ids.Length - 1 && _ids[id] == null)
            {
                return true;
            }

            return false;
        }

        public bool PacketExists(ushort id)
        {
            var index = (int)id % _windowSize;
            if (_ids[index] == id)
            {
                var packet = _networkPackets[index];
                if (!packet.HasValue)
                {
                    return false;
                }

                return true;
            }

            return false;
        }

        public bool IsDelivered(ushort id)
        {
            return _networkPackets[id].HasValue && _networkPackets[id].Value.Acked;
        }

        public bool AcceptAck(
            ushort id,
            uint acks)
        {
            var packet = _networkPackets[id];
            if (!packet.HasValue)
            {
                return false;
            }

            _networkPackets[id] = new PacketData(
                id: id,
                acks: acks,
                acked: true);

            return true;
        }

        public void InsertPacketData(
            ushort id,
            uint acks,
            bool acked)
        {
            var index = (int)id % _windowSize;
            _ids[index] = id;

            _networkPackets[index] = new PacketData(
                id: id,
                acks: acks,
                acked: acked);

            if (NetworkUtils.SequenceGreaterThan(id, _maxId))
            {
                _maxId = id;
            }
        }
    }
}