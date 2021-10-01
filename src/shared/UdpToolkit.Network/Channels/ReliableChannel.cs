namespace UdpToolkit.Network.Channels
{
    using System;
    using UdpToolkit.Network.Contracts.Channels;
    using UdpToolkit.Network.Contracts.Protocol;

    /// <summary>
    /// Reliable UDP channel.
    /// </summary>
    public sealed class ReliableChannel : IChannel
    {
        /// <summary>
        /// Reserved chanel identifier.
        /// </summary>
        public static readonly byte Id = ReliableChannelConsts.ReliableChannel;

        private readonly NetWindow _netWindow;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReliableChannel"/> class.
        /// </summary>
        /// <param name="windowSize">Network window size.</param>
        public ReliableChannel(
            int windowSize)
        {
            _netWindow = new NetWindow(windowSize);
        }

        /// <inheritdoc />
        public bool ResendOnHeartbeat { get; } = true;

        /// <inheritdoc />
        public byte ChannelId { get; } = Id;

        /// <inheritdoc />
        public bool HandleInputPacket(
            in NetworkHeader networkHeader)
        {
            if (_netWindow.GetPacketData(networkHeader.Id))
            {
                return false;
            }

            _netWindow.InsertPacketData(
                id: networkHeader.Id,
                acknowledged: true);

            return true;
        }

        /// <inheritdoc />
        public bool IsDelivered(
            in NetworkHeader networkHeader)
        {
            return _netWindow.GetPacketData(networkHeader.Id);
        }

        /// <inheritdoc />
        public NetworkHeader HandleOutputPacket(
            byte dataType,
            Guid connectionId,
            PacketType packetType)
        {
            var id = _netWindow.GetNextPacketId();
            var acks = FillAcks();

            _netWindow.InsertPacketData(
                id: id,
                acknowledged: false);

            return new NetworkHeader(
                channelId: Id,
                id: id,
                acks: acks,
                connectionId: connectionId,
                packetType: packetType,
                dataType: dataType);
        }

        /// <inheritdoc />
        public bool HandleAck(
            in NetworkHeader networkHeader)
        {
            if (!_netWindow.GetPacketData(networkHeader.Id))
            {
                _netWindow.InsertPacketData(networkHeader.Id, true);
                return true;
            }

            return false;
        }

#pragma warning disable S3400
        private uint FillAcks()
#pragma warning restore S3400
        {
            // not supported
            return 0;
        }
    }
}