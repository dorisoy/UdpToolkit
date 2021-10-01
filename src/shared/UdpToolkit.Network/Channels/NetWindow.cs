namespace UdpToolkit.Network.Channels
{
    /// <summary>
    /// Sliding network window.
    /// </summary>
    public sealed class NetWindow
    {
        private readonly int _windowSize;
        private readonly ushort[] _ids;
        private readonly bool[] _acks;

        private ushort _maxId;

        /// <summary>
        /// Initializes a new instance of the <see cref="NetWindow"/> class.
        /// </summary>
        /// <param name="windowSize">Size of buffer in network window.</param>
        public NetWindow(int windowSize)
        {
            _windowSize = windowSize;
            _ids = new ushort[windowSize];
            _acks = new bool[windowSize];
        }

        /// <summary>
        /// Generate next packet id.
        /// </summary>
        /// <returns>Packet id.</returns>
        public ushort GetNextPacketId() => ++_maxId;

        /// <summary>
        /// Check the delivery status of a packet.
        /// </summary>
        /// <param name="id">Packet id.</param>
        /// <returns>
        /// true - packet delivered
        /// false - packet not delivered.
        /// </returns>
        public bool GetPacketData(ushort id)
        {
            var index = id % _windowSize;
            return _ids[index] == id && _acks[index];
        }

        /// <summary>
        /// Insert packet data in network window.
        /// </summary>
        /// <param name="id">Packet ids.</param>
        /// <param name="acknowledged">State of packet delivery.</param>
        public void InsertPacketData(
            ushort id,
            bool acknowledged)
        {
            var index = id % _windowSize;
            _ids[index] = id;
            _acks[index] = acknowledged;

            if (NetworkUtils.SequenceGreaterThan(id, _maxId))
            {
                _maxId = id;
            }
        }
    }
}