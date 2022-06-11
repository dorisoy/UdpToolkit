namespace UdpToolkit.Network.Contracts.Connections
{
    using System;
    using UdpToolkit.Network.Contracts.Sockets;

    /// <summary>
    /// Abstraction for create connection with all needed dependencies.
    /// </summary>
    public interface IConnectionFactory
    {
        /// <summary>
        /// Create a connection.
        /// </summary>
        /// <param name="connectionId">Connection identifier.</param>
        /// <param name="keepAlive">Flag indicating whether needs to remove the connection from the pool on cleanup scan.</param>
        /// <param name="createdAt">Creation date time.</param>
        /// <param name="ipAddress">Ip address of connection.</param>
        /// <returns>Connection instance.</returns>
        IConnection Create(
            Guid connectionId,
            bool keepAlive,
            DateTimeOffset createdAt,
            IpV4Address ipAddress);
    }
}