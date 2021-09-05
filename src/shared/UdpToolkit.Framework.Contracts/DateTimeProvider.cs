namespace UdpToolkit.Framework.Contracts
{
    using System;

    /// <inheritdoc />
    public sealed class DateTimeProvider : IDateTimeProvider
    {
        /// <inheritdoc />
        public DateTimeOffset GetUtcNow()
        {
            return DateTimeOffset.UtcNow;
        }
    }
}