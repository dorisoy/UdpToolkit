namespace UdpToolkit.Tests.Fakes
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using UdpToolkit.Utils;

    public class FakeCache : ExpiredCacheBase
    {
        private readonly ConcurrentDictionary<Guid, FakeCacheEntry> _cache = new ConcurrentDictionary<Guid, FakeCacheEntry>();

        public FakeCache(
            IDateTimeProvider dateTimeProvider,
            TimeSpan cacheEntryTtl,
            TimeSpan scanFrequency)
            : base(dateTimeProvider, cacheEntryTtl, scanFrequency)
        {
        }

        public void Add(IEnumerable<FakeCacheEntry> entries)
        {
            foreach (var entry in entries)
            {
                _cache.TryAdd(entry.Id, entry);
            }

            StartExpirationScan(_cache);
        }

        public IEnumerable<FakeCacheEntry> GetEntries()
        {
            var entries = _cache.Values.Where(value => !value.IsExpired(DateTimeProvider.UtcNow(), CacheEntryTtl));

            StartExpirationScan(_cache);

            return entries;
        }
    }
}