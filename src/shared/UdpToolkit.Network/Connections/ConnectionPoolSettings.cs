namespace UdpToolkit.Network.Connections
{
    using System;

    /// <summary>
    /// Connection pool settings.
    /// </summary>
    public class ConnectionPoolSettings
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionPoolSettings"/> class.
        /// </summary>
        /// <param name="connectionTimeout">Connection timeout value for inactive connections.</param>
        /// <param name="connectionsCleanupFrequency">Connections cleanup frequency value.</param>
        public ConnectionPoolSettings(
            TimeSpan connectionTimeout,
            TimeSpan connectionsCleanupFrequency)
        {
            ConnectionTimeout = connectionTimeout;
            ConnectionsCleanupFrequency = connectionsCleanupFrequency;
        }

        /// <summary>
        /// Gets value of connection timeout.
        /// </summary>
        public TimeSpan ConnectionTimeout { get; }

        /// <summary>
        /// Gets value of connection cleanup frequency.
        /// </summary>
        public TimeSpan ConnectionsCleanupFrequency { get; }
    }
}