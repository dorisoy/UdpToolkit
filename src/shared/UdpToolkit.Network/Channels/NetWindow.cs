namespace UdpToolkit.Network.Channels
{
    using System;
    using System.Collections.Generic;
    using UdpToolkit.Network.Packets;

    public sealed class NetWindow
    {
        private readonly int _windowSize;
        private readonly ushort?[] _ids;
        private readonly PacketData?[] _networkPackets;
        private ushort _minAckedPacket = 0;

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

        public bool TryGetNetworkPacket(ushort id, out NetworkPacket networkPacket)
        {
            networkPacket = default;
            var index = (int)id % _windowSize;
            if (_ids[index] == id)
            {
                var packet = _networkPackets[index];
                if (!packet.HasValue)
                {
                    return false;
                }

                networkPacket = packet.Value.NetworkPacket;

                return true;
            }

            return false;
        }

        public bool IsAcked(ushort id)
        {
            return _networkPackets[id].HasValue && _networkPackets[id].Value.Acked;
        }

        public IEnumerable<NetworkPacket> GetLostPackets()
        {
            for (var i = _minAckedPacket; i < _networkPackets.Length; i++)
            {
                var packet = _networkPackets[i];
                if (!packet.HasValue)
                {
                    continue;
                }

                var isExpired = packet.Value.NetworkPacket.IsExpired();
                if ((packet.Value.Acked && i == _minAckedPacket) || isExpired)
                {
                    _minAckedPacket++;
                    if (isExpired)
                    {
                        Console.WriteLine("NoAckCallback");
                        packet.Value.NetworkPacket.NoAckCallback();
                    }
                }
                else
                {
                    yield return packet.Value.NetworkPacket;
                }
            }

            Console.WriteLine($"Min packet {_minAckedPacket}");
        }

        public void Ack(ushort id)
        {
            var packet = _networkPackets[id];
            if (!packet.HasValue)
            {
                return;
            }

            _networkPackets[id] = new PacketData(
                networkPacket: packet.Value.NetworkPacket,
                acked: true);
        }

        public void InsertPacketData(NetworkPacket networkPacket)
        {
            var index = (int)networkPacket.ChannelHeader.Id % _windowSize;
            _ids[index] = networkPacket.ChannelHeader.Id;

            _networkPackets[index] = new PacketData(
                networkPacket,
                acked: false);

            if (networkPacket.ChannelHeader.Id > _maxId)
            {
                _maxId = networkPacket.ChannelHeader.Id;
            }
        }
    }
}