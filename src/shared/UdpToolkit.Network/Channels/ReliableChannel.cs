namespace UdpToolkit.Network.Channels
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using Serilog;
    using UdpToolkit.Network.Packets;

    public sealed class ReliableChannel : IChannel
    {
        private readonly ILogger _logger = Log.Logger.ForContext<ReliableChannel>();
        private readonly NetWindow _netWindow;

        public ReliableChannel(int windowSize)
        {
            _netWindow = new NetWindow(windowSize);
        }

        public bool HandleInputPacket(
            NetworkPacket networkPacket)
        {
            var id = networkPacket.ChannelHeader.Id;
            if (!_netWindow.CanSet(id))
            {
                return false;
            }

            _netWindow.InsertPacketData(networkPacket: networkPacket, acked: true);

            return true;
        }

        public NetworkPacket GetAck(
            NetworkPacket networkPacket,
            IPEndPoint ipEndPoint)
        {
            return new NetworkPacket(
                channelHeader: new ChannelHeader(
                    id: networkPacket.ChannelHeader.Id,
                    acks: networkPacket.ChannelHeader.Acks),
                serializer: () => Array.Empty<byte>(),
                ipEndPoint: ipEndPoint,
                hookId: networkPacket.HookId,
                channelType: networkPacket.ChannelType,
                peerId: networkPacket.PeerId,
                resendTimeout: networkPacket.ResendTimeout,
                createdAt: networkPacket.CreatedAt,
                networkPacketType: NetworkPacketType.Ack);
        }

        public void HandleOutputPacket(
            NetworkPacket networkPacket)
        {
            var exists = _netWindow.TryGetNetworkPacket(networkPacket.ChannelHeader.Id, out var packet);

            var sendingPacket = exists
                ? packet
                : networkPacket.SetChannelHeader(new ChannelHeader(
                    id: _netWindow.Next(),
                    acks: FillAcks()));

            if (!exists)
            {
                _netWindow.InsertPacketData(networkPacket: sendingPacket, acked: false);
            }
        }

        public bool HandleAck(NetworkPacket networkPacket)
        {
            if (!_netWindow.IsAcked(networkPacket.ChannelHeader.Id))
            {
                return _netWindow.AcceptAck(networkPacket.ChannelHeader.Id) != null;
            }

            return false;
        }

        public IEnumerable<NetworkPacket> ToResend()
        {
            return _netWindow.GetLostPackets();
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