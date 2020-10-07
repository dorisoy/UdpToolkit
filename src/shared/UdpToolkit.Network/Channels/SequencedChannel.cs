namespace UdpToolkit.Network.Channels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using UdpToolkit.Network.Packets;

    public sealed class SequencedChannel : IChannel
    {
        private readonly HashSet<ushort> _received = new HashSet<ushort>();
        private ushort _sequenceNumber;

        public SequencedChannel()
        {
            _sequenceNumber = 0;
        }

        public bool HandleInputPacket(
            NetworkPacket networkPacket)
        {
            var packetId = networkPacket.ChannelHeader.Id;
            if (packetId <= _sequenceNumber && _received.Contains(item: packetId))
            {
                return false;
            }

            _received.Add(item: packetId);

            return true;
        }

        public NetworkPacket GetAck(
            NetworkPacket networkPacket,
            IPEndPoint ipEndPoint)
        {
            throw new NotImplementedException();
        }

        public void HandleOutputPacket(
            NetworkPacket networkPacket)
        {
            _sequenceNumber++;

            networkPacket.SetChannelHeader(new ChannelHeader(
                id: _sequenceNumber,
                acks: 0));
        }

        public bool HandleAck(
            NetworkPacket networkPacket)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NetworkPacket> ToResend()
        {
            return Enumerable.Empty<NetworkPacket>();
        }
    }
}