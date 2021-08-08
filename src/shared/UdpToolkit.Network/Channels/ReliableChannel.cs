namespace UdpToolkit.Network.Channels
{
    using UdpToolkit.Network.Contracts.Channels;

    public sealed class ReliableChannel : IChannel
    {
        public static readonly byte Id = ReliableChannelConsts.ReliableChannel;

        private readonly NetWindow _netWindow;

        public ReliableChannel(
            int windowSize)
        {
            _netWindow = new NetWindow(windowSize);
        }

        public bool IsReliable { get; } = true;

        public byte ChannelId { get; } = Id;

        public bool HandleInputPacket(
            ushort id,
            uint acks)
        {
            if (!_netWindow.CanSet(id))
            {
                return false;
            }

            _netWindow.InsertPacketData(
                id: id,
                acks: acks,
                acked: true);

            return true;
        }

        public bool IsDelivered(
            ushort id)
        {
            return _netWindow.IsDelivered(id);
        }

        public void HandleOutputPacket(
            out ushort id,
            out uint acks)
        {
            id = _netWindow.Next();
            acks = FillAcks();

            _netWindow.InsertPacketData(
                id: id,
                acks: acks,
                acked: false);
        }

        public bool HandleAck(
            ushort id,
            uint acks)
        {
            if (!_netWindow.IsDelivered(id))
            {
                return _netWindow.AcceptAck(
                    id: id,
                    acks: acks);
            }

            return false;
        }

#pragma warning disable S3400
        private uint FillAcks()
#pragma warning restore S3400
        {
            // not supported right now
            return 0;
        }
    }
}