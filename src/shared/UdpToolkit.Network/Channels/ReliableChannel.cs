namespace UdpToolkit.Network.Channels
{
    using System;
    using System.Collections.Generic;
    using UdpToolkit.Network.Packets;

    public sealed class ReliableChannel : IChannel
    {
        private readonly NetWindow _netWindow;
        private readonly object _locker = new object();

        public ReliableChannel(int windowSize)
        {
            _netWindow = new NetWindow(windowSize);
        }

        public bool HandleInputPacket(
            NetworkPacket networkPacket)
        {
            lock (_locker)
            {
                var id = networkPacket.Id;
                if (!_netWindow.CanSet(id))
                {
                    return false;
                }

                _netWindow.InsertPacketData(networkPacket: networkPacket, acked: true);

                return true;
            }
        }

        public NetworkPacket GetAck(
            NetworkPacket networkPacket)
        {
            lock (_locker)
            {
                return new NetworkPacket(
                    id: networkPacket.Id,
                    acks: networkPacket.Acks,
                    serializer: Array.Empty<byte>,
                    createdAt: networkPacket.CreatedAt,
                    ipEndPoint: networkPacket.IpEndPoint,
                    hookId: networkPacket.HookId,
                    channelType: networkPacket.ChannelType,
                    peerId: networkPacket.PeerId,
                    networkPacketType: NetworkPacketType.Ack);
            }
        }

        public void HandleOutputPacket(
            NetworkPacket networkPacket)
        {
            lock (_locker)
            {
                var exists = _netWindow.TryGetNetworkPacket(
                    id: networkPacket.Id,
                    networkPacket: out var packet);

                if (!exists)
                {
                    networkPacket.SetHeader(id: _netWindow.Next(), acks: FillAcks());
                    _netWindow.InsertPacketData(
                        networkPacket: networkPacket,
                        acked: false);
                }
            }
        }

        public void GetNext(
            NetworkPacket networkPacket)
        {
            lock (_locker)
            {
                networkPacket.SetHeader(id: _netWindow.Next(), acks: FillAcks());
                _netWindow.InsertPacketData(
                    networkPacket: networkPacket,
                    acked: false);
            }
        }

        public bool HandleAck(NetworkPacket networkPacket)
        {
            lock (_locker)
            {
                if (!_netWindow.IsAcked(networkPacket.Id))
                {
                    return _netWindow.AcceptAck(networkPacket.Id) != null;
                }

                return false;
            }
        }

        public IEnumerable<NetworkPacket> ToResend(TimeSpan resendTimeout)
        {
            lock (_locker)
            {
                return _netWindow.GetLostPackets(resendTimeout);
            }
        }

        private uint FillAcks()
        {
            uint acks = 0;

            for (ushort i = 0; i < 32; i++)
            {
                if (_netWindow.TryGetNetworkPacket(i, out var networkPacket))
                {
                    NetworkUtils.SetBitValue(ref acks, i);
                }
            }

            return acks;
        }
    }
}