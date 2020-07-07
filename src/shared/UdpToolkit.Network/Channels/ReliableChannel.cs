namespace UdpToolkit.Network.Channels
{
    using System.Collections.Generic;
    using System.Linq;
    using Serilog;
    using UdpToolkit.Network.Packets;
    using UdpToolkit.Network.Queues;

    public sealed class ReliableChannel : IChannel
    {
        private readonly ILogger _logger = Log.Logger.ForContext<ReliableChannel>();

        private readonly Queue<NetworkPacket> _pendingSends = new Queue<NetworkPacket>();
        private readonly NetWindow _netWindow;

        public ReliableChannel(int windowSize)
        {
            _netWindow = new NetWindow(windowSize);
        }

        public ChannelResult TryHandleInputPacket(
            NetworkPacket networkPacket)
        {
            var id = networkPacket.ChannelHeader.Id;

            if (!_netWindow.CanSet(id))
            {
                // ack again
                _netWindow.TryGetNetworkPacket(id, out var toResend);

                return new ChannelResult(channelState: ChannelState.Resend, networkPacket: toResend);
            }

            _netWindow.InsertPacketData(networkPacket);

            return new ChannelResult(channelState: ChannelState.Accepted, networkPacket: networkPacket);
        }

        public NetworkPacket? TryHandleOutputPacket(
            NetworkPacket networkPacket)
        {
            var isAck = (PacketType)networkPacket.HookId == PacketType.Ack;
            if (isAck)
            {
                var newPacket = new NetworkPacket(
                    peerId: networkPacket.PeerId,
                    channelHeader: new ChannelHeader(
                        id: networkPacket.ChannelHeader.Id,
                        acks: FillAcks()),
                    serializer: networkPacket.Serializer,
                    ipEndPoint: networkPacket.IpEndPoint,
                    hookId: networkPacket.HookId,
                    channelType: networkPacket.ChannelType);

                _netWindow.Ack(newPacket.ChannelHeader.Id);

                return newPacket;
            }
            else
            {
                var newPacket = new NetworkPacket(
                    peerId: networkPacket.PeerId,
                    channelHeader: new ChannelHeader(
                        id: _netWindow.Next(),
                        acks: FillAcks()),
                    serializer: networkPacket.Serializer,
                    ipEndPoint: networkPacket.IpEndPoint,
                    hookId: networkPacket.HookId,
                    channelType: networkPacket.ChannelType);

                if (!_netWindow.CanSet(newPacket.ChannelHeader.Id) && newPacket.HookId < (byte)PacketType.Ping)
                {
                    _logger.Warning("Outgoing packet window is exhausted. Expect delays");
                    _pendingSends.Enqueue(item: newPacket);

                    return null;
                }

                _netWindow.InsertPacketData(newPacket);

                return newPacket;
            }
        }

        public NetworkPacket? HandleAck(NetworkPacket networkPacket)
        {
            if (!_netWindow.IsAcked(networkPacket.ChannelHeader.Id))
            {
                _netWindow.Ack(networkPacket.ChannelHeader.Id);

                return networkPacket;
            }

            return null;
        }

        public IEnumerable<NetworkPacket> GetPendingPackets()
        {
            return _pendingSends.AsEnumerable();
        }

        public void Resend(IAsyncQueue<NetworkPacket> outputQueue)
        {
            foreach (var pendingPacket in _pendingSends)
            {
                outputQueue.Produce(@event: pendingPacket);
            }

            for (ushort i = 0; i < _netWindow.Size; i++)
            {
                // TODO check timeouts and attempts
                if (_netWindow.TryGetNetworkPacket(i, out var networkPacket))
                {
                    outputQueue.Produce(networkPacket);
                }
            }
        }

        public void Flush()
        {
            _pendingSends.Clear();
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