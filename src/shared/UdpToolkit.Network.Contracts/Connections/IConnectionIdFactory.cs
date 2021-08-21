namespace UdpToolkit.Network.Contracts.Connections
{
    using System;

    public interface IConnectionIdFactory
    {
        Guid Generate();
    }
}