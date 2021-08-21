namespace UdpToolkit.Framework.Contracts
{
    using System;

    public interface IDateTimeProvider
    {
        DateTimeOffset GetUtcNow();
    }
}