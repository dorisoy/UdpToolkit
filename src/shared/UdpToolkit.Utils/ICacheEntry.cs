namespace UdpToolkit.Utils
{
    using System;

    public interface ICacheEntry
    {
        bool IsExpired(DateTimeOffset now, TimeSpan ttl);
    }
}