namespace UdpToolkit.Utils
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public abstract class ExpiredCacheBase
    {
        private DateTimeOffset _lastScan;

        protected ExpiredCacheBase(
            IDateTimeProvider dateTimeProvider,
            TimeSpan cacheEntryTtl,
            TimeSpan scanFrequency)
        {
            DateTimeProvider = dateTimeProvider;
            _lastScan = DateTimeProvider.UtcNow();
            CacheEntryTtl = cacheEntryTtl;
            ScanFrequency = scanFrequency;
        }

        protected TimeSpan CacheEntryTtl { get; }

        protected TimeSpan ScanFrequency { get; }

        protected IDateTimeProvider DateTimeProvider { get; }

        protected void StartExpirationScan<TKey, TValue>(IDictionary<TKey, TValue> dictionary)
            where TValue : ICacheEntry
        {
            var now = DateTimeProvider.UtcNow();
            if (ScanFrequency < now - _lastScan)
            {
                _lastScan = now;
                Task.Factory.StartNew(
                    action: state => RemoveExpiredItems(dictionary: dictionary),
                    state: this,
                    cancellationToken: default,
                    creationOptions: TaskCreationOptions.DenyChildAttach,
                    scheduler: TaskScheduler.Default);
            }
        }

        private void RemoveExpiredItems<TKey, TValue>(IDictionary<TKey, TValue> dictionary)
            where TValue : ICacheEntry
        {
            var now = DateTimeProvider.UtcNow();
            foreach (var pair in dictionary)
            {
                if (pair.Value.IsExpired(now, CacheEntryTtl))
                {
                    dictionary.Remove(pair.Key);
                }
            }
        }
    }
}