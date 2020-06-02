namespace UdpToolkit.Framework.Server.Peers
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using UdpToolkit.Framework.Server.Core;
    using UdpToolkit.Utils;

    public sealed class PeerScopeTracker : ExpiredCacheBase, IPeerScopeTracker
    {
        private readonly ConcurrentDictionary<ushort, IPeerScope> _scopes = new ConcurrentDictionary<ushort, IPeerScope>();

        public PeerScopeTracker(
            IDateTimeProvider dateTimeProvider,
            TimeSpan cacheEntryTtl,
            TimeSpan scanFrequency)
            : base(dateTimeProvider, cacheEntryTtl, scanFrequency)
        {
        }

        public bool TryGetScope(ushort scopeId, out IPeerScope scope)
        {
            var result = _scopes.TryGetValue(scopeId, out scope);
            var now = DateTimeProvider.UtcNow();

            if (result && scope.IsExpired(now, CacheEntryTtl))
            {
                _scopes.Remove(scope.ScopeId, out var removed);
                return false;
            }

            StartExpirationScan(_scopes);

            return result;
        }

        public IPeerScope GetOrAddScope(ushort scopeId, IPeerScope peerScope)
        {
            var now = DateTimeProvider.UtcNow();
            var lazy = new Lazy<PeerScope>((PeerScope)peerScope);

            var scope = _scopes.GetOrAdd(
                key: scopeId,
                valueFactory: (i) => lazy.Value);

            if (scope.IsExpired(now, CacheEntryTtl))
            {
                _scopes.Remove(key: scope.ScopeId, value: out var removed);

                return null;
            }

            StartExpirationScan(_scopes);

            return scope;
        }
    }
}
