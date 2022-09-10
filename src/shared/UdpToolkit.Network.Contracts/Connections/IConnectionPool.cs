namespace UdpToolkit.Network.Contracts.Connections
{
    using System;
    using System.Collections.Generic;
    using UdpToolkit.Network.Contracts.Sockets;

    /// <summary>
    /// Abstraction for sharing all connections between all instances of UdpClient.
    /// </summary>
    public interface IConnectionPool : IDisposable
    {
        /// <summary>
        /// Remove connection.
        /// </summary>
        /// <param name="connection">Instance of connection.</param>
        void Remove(
            IConnection connection);

        /// <summary>
        /// Try get connection.
        /// </summary>
        /// <param name="connectionId">Connection identifier.</param>
        /// <param name="connection">Instance of connection.</param>
        /// <returns>
        /// true - connection exists
        /// false - connection not exists or expired.
        /// </returns>
        bool TryGetConnection(
            Guid connectionId,
            out IConnection connection);

        /// <summary>
        /// Get or add connection.
        /// </summary>
        /// <param name="connectionId">Connection identifier.</param>
        /// <param name="routingKey">Routing key.</param>
        /// <param name="keepAlive">Flag indicating whether needs to remove the connection from the pool on cleanup scan.</param>
        /// <param name="timestamp">Timestamp (utc now).</param>
        /// <param name="ipV4Address">Ip address of connection.</param>
        /// <returns>Instance of connection.</returns>
        IConnection GetOrAdd(
            Guid connectionId,
            Guid routingKey,
            bool keepAlive,
            DateTimeOffset timestamp,
            IpV4Address ipV4Address);

        /// <summary>
        /// Gets list of connections.
        /// </summary>
        /// <returns>List of connections.</returns>
        IReadOnlyList<IConnection> GetAll();
    }
}