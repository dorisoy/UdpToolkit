namespace UdpToolkit.Framework.Contracts
{
    using System;

    public sealed class DateTimeProvider : IDateTimeProvider
    {
        public DateTimeOffset GetUtcNow()
        {
            return DateTimeOffset.UtcNow;
        }
    }
}