namespace UdpToolkit.Framework.Contracts
{
    using System;
    using UdpToolkit.Network.Contracts.Connections;

    public sealed class ConnectionIdFactory : IConnectionIdFactory
    {
        public Guid Generate() => Guid.NewGuid();
    }
}