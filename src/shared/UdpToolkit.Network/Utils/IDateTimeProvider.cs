namespace UdpToolkit.Network.Utils
{
    using System;

    /// <summary>
    /// Abstraction for providing current Utc DateTimeOffset.
    /// </summary>
    public interface IDateTimeProvider
    {
        /// <summary>
        /// Get utc now.
        /// </summary>
        /// <returns>
        /// DateTimeOffset, UtcNow.
        /// </returns>
        DateTimeOffset GetUtcNow();
    }
}