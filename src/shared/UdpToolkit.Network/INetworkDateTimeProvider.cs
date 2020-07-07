namespace UdpToolkit.Network
{
    using System;

    public interface INetworkDateTimeProvider
    {
        DateTimeOffset UtcNowNetwork();
    }
}