namespace UdpToolkit.Network.Utils
{
    using System;

    /// <inheritdoc />
    public sealed class DateTimeProvider : IDateTimeProvider
    {
        /// <inheritdoc />
        public DateTimeOffset GetUtcNow() => DateTimeOffset.UtcNow;
    }
}