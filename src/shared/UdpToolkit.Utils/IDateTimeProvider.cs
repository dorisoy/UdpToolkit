namespace UdpToolkit.Utils
{
    using System;

    public interface IDateTimeProvider
    {
        DateTimeOffset UtcNow();
    }
}