namespace UdpToolkit.Network.Channels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UdpToolkit.Network.Packets;
    using UdpToolkit.Network.Queues;

    public sealed class SequencedChannel : IChannel
    {
        private readonly HashSet<ushort> _received = new HashSet<ushort>();
        private ushort _sequenceNumber;

        public SequencedChannel()
        {
            _sequenceNumber = 0;
        }

        public ChannelResult TryHandleInputPacket(
            NetworkPacket networkPacket)
        {
            var packetId = networkPacket.ChannelHeader.Id;
            if (packetId <= _sequenceNumber && _received.Contains(item: packetId))
            {
                // drop packet
                return new ChannelResult(channelState: ChannelState.Drop, networkPacket: networkPacket);
            }

            _received.Add(item: packetId);

            return new ChannelResult(channelState: ChannelState.Accepted, networkPacket: networkPacket);
        }

        public NetworkPacket TryHandleOutputPacket(
            NetworkPacket networkPacket)
        {
            _sequenceNumber++;

            return new NetworkPacket(
                peerId: networkPacket.PeerId,
                channelHeader: new ChannelHeader(
                    id: _sequenceNumber,
                    acks: 0),
                channelType: networkPacket.ChannelType,
                serializer: networkPacket.Serializer,
                ipEndPoint: networkPacket.IpEndPoint,
                hookId: networkPacket.HookId);
        }

        public NetworkPacket HandleAck(NetworkPacket networkPacket)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NetworkPacket> GetPendingPackets()
        {
            return Enumerable.Empty<NetworkPacket>();
        }

        public void Resend(IAsyncQueue<NetworkPacket> outputQueue)
        {
        }

        public void Flush()
        {
            throw new NotImplementedException();
        }
    }
}