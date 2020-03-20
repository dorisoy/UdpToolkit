namespace UdpToolkit.Network.Rudp
{
    public sealed class ReliableChannel
    {
        private readonly bool[] _acknowledged;
        private readonly uint[] _sequence;
        private const int BufferSize = 1024;
        
        private uint _localNumber = 0;

        public ReliableChannel()
        {
            _acknowledged = new bool[BufferSize];
            _sequence = new uint[BufferSize];
        }

        public ReliableUdpHeader GetReliableHeader()
        {
            _localNumber++;

            return new ReliableUdpHeader(
                localNumber: _localNumber,
                ack: _localNumber,
                acks: FillAcks(_localNumber));
        }

        public uint FillAcks(uint number)
        {
            uint acks = 0;
            if (number == 0)
            {
                return acks;
            }

            uint index = (number % 1024) - 1;
            for (uint i = 0; index > 0 && i < 31; i++)
            {
                if (PacketAcknowledged(index))
                {
                    acks |= 1u << (int) i;
                }

                index--;
            }

            return acks;
        }

        public bool PacketAcknowledged(uint number)
        {
            uint index = number % BufferSize;
            if (_sequence[index] == number)
                return _acknowledged[index];

            return false;
        }

        public bool InsertPacket(uint number)
        {
            uint index = number % BufferSize;
            _sequence[index] = number;

            return _acknowledged[index];
        }
    }
}
