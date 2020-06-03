namespace UdpToolkit.Network.Peers
{
    using System;
    using System.Net;
    using System.Threading;
    using UdpToolkit.Network.Rudp;
    using UdpToolkit.Utils;

    public sealed class Peer
    {
        private readonly ReliableUdpChannel _reliableUdpChannel;

        public Peer(
            Guid peerId,
            IPEndPoint ipEndPoint,
            ReliableUdpChannel reliableUdpChannel,
            DateTimeOffset lastActivityAt,
            DateTimeOffset createdAt)
        {
            PeerId = peerId;
            IpEndPoint = ipEndPoint;
            LastActivityAt = lastActivityAt;
            CreatedAt = createdAt;
            _reliableUdpChannel = reliableUdpChannel;
        }

        public Guid PeerId { get; }

        public DateTimeOffset CreatedAt { get; }

        public DateTimeOffset LastActivityAt { get; private set; }

        public IPEndPoint IpEndPoint { get; }

        public void InsertPacket(uint number) => _reliableUdpChannel.InsertPacket(number);

        public void UpdateLastActivity(DateTimeOffset lastActivityAt)
        {
            LastActivityAt = lastActivityAt;
        }
    }
}
