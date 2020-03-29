namespace UdpToolkit.Tests
{
    using System;
    using System.Linq;
    using System.Threading;
    using UdpToolkit.Tests.Fakes;
    using Xunit;

    public class ExpiredCacheTests
    {
        [Fact]
        public void ExpiredCache_AddEntries_AddedEntriesNotExpired()
        {
            var dateTimeProvider = new FakeDateTimeProvider("1/25/2020 1:30:30 PM +00:00");
            var now = dateTimeProvider.UtcNow();
            var entryTtl = TimeSpan.FromMinutes(1);

            var cache = new FakeCache(
                dateTimeProvider: dateTimeProvider,
                cacheEntryTtl: entryTtl,
                scanFrequency: TimeSpan.FromMinutes(1));

            var entries = Enumerable
                .Range(0, 10)
                .Select(_ => new FakeCacheEntry(
                    createdAt: now,
                    id: Guid.NewGuid()));

            cache.Add(entries);

            var fromCache = cache.GetEntries().ToArray();

            Assert.Equal(10, fromCache.Count());
            Assert.True(fromCache.All(x => !x.IsExpired(now, entryTtl)));
        }

        [Fact]
        public void ExpiredCache_AddEntries_AddedEntriesExpired()
        {
            var total = 10;

            var createdAt = "1/25/2020 1:30:30 PM +00:00";
            var expiredAt = "1/25/2020 1:35:30 PM +00:00";

            var dateTimeProvider = new FakeDateTimeProvider(date: createdAt);
            var now = dateTimeProvider.UtcNow();
            var entryTtl = TimeSpan.FromMinutes(1);

            var cache = new FakeCache(
                dateTimeProvider: dateTimeProvider,
                cacheEntryTtl: entryTtl,
                scanFrequency: TimeSpan.FromMinutes(1));

            var entries = Enumerable
                .Range(0, total)
                .Select(_ => new FakeCacheEntry(
                    createdAt: now,
                    id: Guid.NewGuid()));

            cache.Add(entries);

            dateTimeProvider.RewindDateTime(expiredAt);

            SpinWait.SpinUntil(() => !cache.GetEntries().Any(), TimeSpan.FromMinutes(1));

            Assert.Empty(cache.GetEntries());
        }
    }
}