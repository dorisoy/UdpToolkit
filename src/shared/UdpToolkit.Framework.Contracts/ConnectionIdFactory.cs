namespace UdpToolkit.Framework.Contracts
{
    using System;

    public sealed class ConnectionIdFactory : IConnectionIdFactory
    {
        public Guid Generate() => Guid.NewGuid();
    }
}