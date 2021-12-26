namespace UdpToolkit.Network.Channels
{
    using UdpToolkit.Network.Contracts.Channels;
    using UdpToolkit.Network.Contracts.Protocol;
    using UdpToolkit.Network.Utils;

    /// <summary>
    /// Reliable UDP channel.
    /// </summary>
    public sealed class ReliableChannel : IChannel
    {
        /// <summary>
        /// Reserved chanel identifier.
        /// </summary>
        public static readonly byte Id = ReliableChannelConsts.ReliableChannel;

        private readonly PacketData[] _netWindow;
        private readonly int _windowSize;
        private ushort _id = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReliableChannel"/> class.
        /// </summary>
        /// <param name="netWindowSize">Size of net window.</param>
        public ReliableChannel(
            int netWindowSize)
        {
            _windowSize = netWindowSize;
            _netWindow = new PacketData[netWindowSize];
        }

        /// <inheritdoc />
        public bool IsReliable { get; } = true;

        /// <inheritdoc />
        public byte ChannelId { get; } = Id;

        /// <inheritdoc />
        public bool HandleInputPacket(
            in NetworkHeader networkHeader)
        {
            var id = networkHeader.Id;
            var index = id % _windowSize;
            ref var packetData = ref _netWindow[index];
            if (packetData.IsDelivered)
            {
                return false;
            }

            packetData.Id = id;
            packetData.IsDelivered = true;

            if (NetworkUtils.SequenceGreaterThan(id, _id))
            {
                _id = id;
            }

            return true;
        }

        /// <inheritdoc />
        public ushort HandleOutputPacket(
            byte dataType)
        {
            var id = ++_id;
            var index = id % _windowSize;
            ref var packetData = ref _netWindow[index];
            packetData.Id = id;
            packetData.IsDelivered = false;

            return _id;
        }

        /// <inheritdoc />
        public bool HandleAck(
            in NetworkHeader networkHeader)
        {
            var index = networkHeader.Id % _windowSize;
            ref var packetData = ref _netWindow[index];
            if (packetData.Id == networkHeader.Id && packetData.IsDelivered)
            {
                return false;
            }

            packetData.Id = networkHeader.Id;
            packetData.IsDelivered = true;

            return true;
        }

        /// <inheritdoc />
        public bool IsDelivered(ushort id)
        {
            var index = id % _windowSize;
            ref var packetData = ref _netWindow[index];
            return packetData.IsDelivered;
        }
    }
}