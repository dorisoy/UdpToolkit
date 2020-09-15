namespace UdpToolkit.Network.Channels
{
    using UdpToolkit.Network.Packets;

    public sealed class NetWindow
    {
        private readonly int _windowSize;
        private readonly ushort?[] _ids;
        private readonly PacketData[] _networkPackets;

        private ushort _maxId;

        public NetWindow(int windowSize)
        {
            _windowSize = windowSize;
            _ids = new ushort?[windowSize];
            _networkPackets = new PacketData[windowSize];
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

        public bool TryGetNetworkPacket(ushort id, out NetworkPacket networkPacket)
        {
            networkPacket = default;
            var index = (int)id % _windowSize;
            if (_ids[index] == id)
            {
                networkPacket = _networkPackets[index].NetworkPacket;

                return true;
            }

            return false;
        }

        public bool IsAcked(ushort id)
        {
            return _networkPackets[id].Acked;
        }

        public void Ack(ushort id)
        {
            var packet = _networkPackets[id];
            _networkPackets[id] = new PacketData(packet.NetworkPacket, true);
        }

        public void InsertPacketData(NetworkPacket networkPacket)
        {
            var index = (int)networkPacket.ChannelHeader.Id % _windowSize;
            _ids[index] = networkPacket.ChannelHeader.Id;
            _networkPackets[index] = new PacketData(networkPacket, false);

            if (networkPacket.ChannelHeader.Id > _maxId)
            {
                _maxId = networkPacket.ChannelHeader.Id;
            }
        }
    }
}