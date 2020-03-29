namespace UdpToolkit.Tests.Fakes
{
    using System;
    using UdpToolkit.Utils;

    public sealed class FakeCacheEntry : ICacheEntry
    {
        public FakeCacheEntry(
            DateTimeOffset createdAt,
            Guid id)
        {
            CreatedAt = createdAt;
            Id = id;
        }

        public Guid Id { get; }

        public DateTimeOffset CreatedAt { get; }

        public bool IsExpired(DateTimeOffset now, TimeSpan ttl)
        {
            return now > CreatedAt + ttl;
        }
    }
}