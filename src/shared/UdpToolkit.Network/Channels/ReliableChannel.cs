namespace UdpToolkit.Network.Channels
{
    using System;
    using System.Collections.Generic;
    using UdpToolkit.Network.Packets;
    using UdpToolkit.Network.Pooling;

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
            NetworkPacket networkPacket)
        {
            lock (_locker)
            {
                var id = networkPacket.Id;
                var acks = networkPacket.Acks;
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

        public void GetAck(
            NetworkPacket networkPacket)
        {
            lock (_locker)
            {
                networkPacket.Set(
                    serializer: Array.Empty<byte>,
                    networkPacketType: NetworkPacketType.Ack);
            }
        }

        public bool IsDelivered(
            ushort networkPacketId)
        {
            lock (_locker)
            {
                return _netWindow.IsDelivered(networkPacketId);
            }
        }

        public void HandleOutputPacket(
            NetworkPacket networkPacket)
        {
            lock (_locker)
            {
                if (!_netWindow.PacketExists(id: networkPacket.Id))
                {
                    networkPacket.Set(id: _netWindow.Next(), acks: FillAcks());
                    _netWindow.InsertPacketData(
                        id: networkPacket.Id,
                        acks: networkPacket.Acks,
                        acked: false);
                }
            }
        }

        public bool HandleAck(
            NetworkPacket networkPacket)
        {
            lock (_locker)
            {
                if (!_netWindow.IsDelivered(networkPacket.Id))
                {
                    return _netWindow.AcceptAck(
                        id: networkPacket.Id,
                        acks: networkPacket.Acks);
                }

                return false;
            }
        }

        private uint FillAcks()
        {
            uint acks = 0;
            for (ushort i = 0; i < 32; i++)
            {
                if (_netWindow.PacketExists(id: i))
                {
                    NetworkUtils.SetBitValue(ref acks, i);
                }
            }

            return acks;
        }
    }
}