namespace UdpToolkit.Utils
{
    using System;

    public sealed class DateTimeProvider : IDateTimeProvider
    {
        public DateTimeOffset UtcNow()
        {
            return DateTime.UtcNow;
        }
    }
}