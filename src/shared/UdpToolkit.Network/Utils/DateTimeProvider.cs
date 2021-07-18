namespace UdpToolkit.Network.Utils
{
    using System;

    public sealed class DateTimeProvider : IDateTimeProvider
    {
        public DateTimeOffset GetUtcNow() => DateTimeOffset.UtcNow;
    }
}