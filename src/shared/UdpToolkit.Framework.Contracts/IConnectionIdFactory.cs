namespace UdpToolkit.Framework.Contracts
{
    using System;

    public interface IConnectionIdFactory
    {
        Guid Generate();
    }
}