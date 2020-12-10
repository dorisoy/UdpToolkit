namespace UdpToolkit.Network.Channels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UdpToolkit.Network.Packets;

    public sealed class SequencedChannel : IChannel
    {
        private readonly object _locker = new object();
        private ushort _lastReceivedNumber;
        private ushort _sequenceNumber;

        public SequencedChannel()
        {
            _sequenceNumber = 0;
        }

        public bool HandleInputPacket(
            NetworkPacket networkPacket)
        {
            lock (_locker)
            {
                var packetId = networkPacket.Id;
                var flag = packetId != _lastReceivedNumber;
                if (NetworkUtils.SequenceGreaterThan(packetId, _sequenceNumber) && flag)
                {
                    _lastReceivedNumber = packetId;
                    return true;
                }

                return false;
            }
        }

        public NetworkPacket GetAck(
            NetworkPacket networkPacket)
        {
            return networkPacket;
        }

        public void HandleOutputPacket(
            NetworkPacket networkPacket)
        {
            lock (_locker)
            {
                networkPacket.SetHeader(
                    id: ++_sequenceNumber,
                    acks: 0);
            }
        }

        public void GetNext(NetworkPacket networkPacket)
        {
        }

        public bool HandleAck(
            NetworkPacket networkPacket)
        {
            return true;
        }

        public IEnumerable<NetworkPacket> ToResend(TimeSpan resendTimeout)
        {
            return Enumerable.Empty<NetworkPacket>();
        }
    }
}