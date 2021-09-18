namespace UdpToolkit.Framework
{
    using System;
    using UdpToolkit.Network.Contracts.Connections;

    /// <summary>
    /// Default implementation of IConnectionIdFactory generates random connection identifiers.
    /// </summary>
    public sealed class ConnectionIdFactory : IConnectionIdFactory
    {
        /// <inheritdoc />
        public Guid Generate() => Guid.NewGuid();
    }
}