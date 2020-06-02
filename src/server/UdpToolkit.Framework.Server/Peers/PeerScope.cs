namespace UdpToolkit.Framework.Server.Peers
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using UdpToolkit.Framework.Server.Core;
    using UdpToolkit.Network.Peers;
    using UdpToolkit.Utils;

    public sealed class PeerScope : ExpiredCacheBase, IPeerScope
    {
        private readonly ConcurrentDictionary<string, Peer> _scope = new ConcurrentDictionary<string, Peer>();
        private readonly DateTimeOffset _createdAt;

        public PeerScope(
            ushort scopeId,
            IDateTimeProvider dateTimeProvider,
            TimeSpan cacheEntryTtl,
            TimeSpan scanFrequency)
            : base(dateTimeProvider, cacheEntryTtl, scanFrequency)
        {
            ScopeId = scopeId;
            _createdAt = dateTimeProvider.UtcNow();
        }

        public ushort ScopeId { get; }

        public void AddPeer(Peer peer)
        {
            _scope.TryAdd(peer.Id, peer);

            StartExpirationScan(_scope);
        }

        public IEnumerable<Peer> GetPeers()
        {
            var now = DateTimeProvider.UtcNow();
            var peers = _scope
                .Select(x => x.Value)
                .Where(x => !x.IsExpired(now, CacheEntryTtl));

            StartExpirationScan(_scope);

            return peers;
        }

        public bool TryGetPeer(string peerId, out Peer peer)
        {
            var result = _scope.TryGetValue(peerId, out peer);
            var now = DateTimeProvider.UtcNow();

            if (result && peer.IsExpired(now, CacheEntryTtl))
            {
                peer = null;
                return false;
            }

            StartExpirationScan(_scope);

            return result;
        }

        public bool IsExpired(DateTimeOffset now, TimeSpan ttl)
        {
            if (ttl == Timeout.InfiniteTimeSpan)
            {
                return false;
            }

            return now > _createdAt + ttl;
        }
    }
}
