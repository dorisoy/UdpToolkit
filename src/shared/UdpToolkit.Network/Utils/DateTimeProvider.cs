namespace UdpToolkit.Network.Utils
{
    using System;

    public class DateTimeProvider : IDateTimeProvider
    {
        public DateTimeOffset GetUtcNow() => DateTimeOffset.UtcNow;
    }
}