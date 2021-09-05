namespace UdpToolkit.Network.Channels
{
    /// <summary>
    /// Sliding network window.
    /// </summary>
    public sealed class NetWindow
    {
        private readonly int _windowSize;
        private readonly ushort?[] _ids;
        private readonly PacketData?[] _packetsData;

        private ushort _maxId;

        /// <summary>
        /// Initializes a new instance of the <see cref="NetWindow"/> class.
        /// </summary>
        /// <param name="windowSize">Size of buffer in network window.</param>
        public NetWindow(int windowSize)
        {
            _windowSize = windowSize;
            _ids = new ushort?[windowSize];
            _packetsData = new PacketData?[windowSize];
        }

        /// <summary>
        /// Generate next packet id.
        /// </summary>
        /// <returns>Packet id.</returns>
        public ushort Next() => ++_maxId;

        /// <summary>
        /// Check free slot in network window before insert.
        /// </summary>
        /// <param name="id">Packet id.</param>
        /// <returns>
        /// true - can set
        /// false - can't set.
        /// </returns>
        public bool CanSet(ushort id)
        {
            if (id <= _ids.Length - 1 && _ids[id] == null)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Check the delivery status of a packet.
        /// </summary>
        /// <param name="id">Packet id.</param>
        /// <returns>
        /// true - packet delivered
        /// false - packet not delivered.
        /// </returns>
        public bool IsDelivered(ushort id)
        {
            return _packetsData[id].HasValue && _packetsData[id].Value.Acked;
        }

        /// <summary>
        /// Try accept ack.
        /// </summary>
        /// <param name="id">Packet id.</param>
        /// <param name="acks">32 bit packet history.</param>
        /// <returns>
        /// true - ack accepted
        /// false - ack not accepted.
        /// </returns>
        public bool TryAcceptAck(
            ushort id,
            uint acks)
        {
            var packet = _packetsData[id];
            if (!packet.HasValue)
            {
                return false;
            }

            _packetsData[id] = new PacketData(
                id: id,
                acks: acks,
                acked: true);

            return true;
        }

        /// <summary>
        /// Insert packet data in network window.
        /// </summary>
        /// <param name="id">Packet ids.</param>
        /// <param name="acks">32 bits history of packet.</param>
        /// <param name="acked">Flag for represent state of packet.</param>
        public void InsertPacketData(
            ushort id,
            uint acks,
            bool acked)
        {
            var index = (int)id % _windowSize;
            _ids[index] = id;

            _packetsData[index] = new PacketData(
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