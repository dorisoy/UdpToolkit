namespace UdpToolkit.Core
{
    using System;

    public interface IDateTimeProvider
    {
        DateTimeOffset UtcNow();
    }
}