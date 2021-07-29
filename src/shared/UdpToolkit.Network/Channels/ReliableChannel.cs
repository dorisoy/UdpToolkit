namespace UdpToolkit.Network.Channels
{
    public sealed class ReliableChannel : IChannel
    {
        private readonly NetWindow _netWindow;
        private readonly object _locker = new object();

        public ReliableChannel(
            int windowSize)
        {
            _netWindow = new NetWindow(windowSize);
        }

        public bool HandleInputPacket(
            ushort id,
            uint acks)
        {
            lock (_locker)
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
        }

        public bool IsDelivered(
            ushort id)
        {
            lock (_locker)
            {
                return _netWindow.IsDelivered(id);
            }
        }

        public void HandleOutputPacket(
            out ushort id,
            out uint acks)
        {
            lock (_locker)
            {
                id = _netWindow.Next();
                acks = FillAcks();

                _netWindow.InsertPacketData(
                    id: id,
                    acks: acks,
                    acked: false);
            }
        }

        public bool HandleAck(
            ushort id,
            uint acks)
        {
            lock (_locker)
            {
                if (!_netWindow.IsDelivered(id))
                {
                    return _netWindow.AcceptAck(
                        id: id,
                        acks: acks);
                }

                return false;
            }
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