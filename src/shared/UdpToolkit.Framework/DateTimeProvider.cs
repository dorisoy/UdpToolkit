namespace UdpToolkit.Framework
{
    using System;
    using UdpToolkit.Core;
    using UdpToolkit.Network;

    public sealed class DateTimeProvider : IDateTimeProvider, INetworkDateTimeProvider
    {
        public DateTimeOffset UtcNow()
        {
            return DateTimeOffset.UtcNow;
        }

        public DateTimeOffset UtcNowNetwork()
        {
            return DateTimeOffset.UtcNow;
        }
    }
}