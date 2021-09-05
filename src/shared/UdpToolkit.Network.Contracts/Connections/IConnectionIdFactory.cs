namespace UdpToolkit.Network.Contracts.Connections
{
    using System;

    /// <summary>
    /// Abstraction for providing a connection identifier for a new connection.
    /// </summary>
    public interface IConnectionIdFactory
    {
        /// <summary>
        /// Generate new connectionId.
        /// </summary>
        /// <returns>ConnectionId.</returns>
        Guid Generate();
    }
}