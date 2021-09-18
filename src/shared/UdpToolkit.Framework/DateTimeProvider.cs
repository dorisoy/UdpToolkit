namespace UdpToolkit.Framework
{
    using System;
    using UdpToolkit.Framework.Contracts;

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