namespace UdpToolkit.Network.Connections
{
    using System;
    using UdpToolkit.Network.Contracts.Sockets;

    /// <summary>
    /// Abstraction for create connection with all needed dependencies.
    /// </summary>
    internal interface IConnectionFactory
    {
        /// <summary>
        /// Create a connection.
        /// </summary>
        /// <param name="connectionId">Connection identifier.</param>
        /// <param name="keepAlive">Flag indicating whether needs to remove the connection from the pool on cleanup scan.</param>
        /// <param name="lastHeartbeat">Last heartbeat (init value).</param>
        /// <param name="ipAddress">Ip address of connection.</param>
        /// <returns>Connection instance.</returns>
        IConnection Create(
            Guid connectionId,
            bool keepAlive,
            DateTimeOffset lastHeartbeat,
            IpV4Address ipAddress);
    }
}