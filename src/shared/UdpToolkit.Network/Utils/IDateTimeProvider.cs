namespace UdpToolkit.Network.Utils
{
    using System;

    public interface IDateTimeProvider
    {
        DateTimeOffset GetUtcNow();
    }
}