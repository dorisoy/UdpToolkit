namespace UdpToolkit.Network.Peers
{
    using System;
    using System.Net;
    using System.Threading;
    using UdpToolkit.Network.Rudp;
    using UdpToolkit.Utils;

    public sealed class Peer : ICacheEntry
    {
        private readonly ReliableUdpChannel _reliableUdpChannel;

        public Peer(
            string id,
            IPEndPoint ipEndPoint,
            ReliableUdpChannel reliableUdpChannel,
            DateTimeOffset lastActivityAt,
            DateTimeOffset createdAt)
        {
            Id = id;
            IpEndPoint = ipEndPoint;
            LastActivityAt = lastActivityAt;
            CreatedAt = createdAt;
            _reliableUdpChannel = reliableUdpChannel;
        }

        public string Id { get; }

        public DateTimeOffset CreatedAt { get; }

        public DateTimeOffset LastActivityAt { get; private set; }

        public IPEndPoint IpEndPoint { get; }

        public ReliableUdpHeader GetReliableHeader() => _reliableUdpChannel.GetReliableHeader();

        public void InsertPacket(uint number) => _reliableUdpChannel.InsertPacket(number);

        public void UpdateLastActivity(DateTimeOffset lastActivityAt)
        {
            LastActivityAt = lastActivityAt;
        }

        public bool IsExpired(DateTimeOffset now, TimeSpan ttl)
        {
            if (ttl == Timeout.InfiniteTimeSpan)
            {
                return false;
            }

            var diff = now - LastActivityAt;
            if (diff == TimeSpan.Zero)
            {
                return false;
            }

            return diff > ttl;
        }
    }
}
