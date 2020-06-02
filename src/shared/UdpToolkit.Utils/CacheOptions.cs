namespace UdpToolkit.Utils
{
    using System;

    public class CacheOptions
    {
        public CacheOptions(
            TimeSpan scanForExpirationFrequency,
            TimeSpan cacheEntryTtl)
        {
            ScanForExpirationFrequency = scanForExpirationFrequency;
            CacheEntryTtl = cacheEntryTtl;
        }

        public CacheOptions()
        {
            ScanForExpirationFrequency = TimeSpan.FromMinutes(1);
            CacheEntryTtl = TimeSpan.FromMinutes(10);
        }

        public TimeSpan ScanForExpirationFrequency { get; }

        public TimeSpan CacheEntryTtl { get; }
    }
}