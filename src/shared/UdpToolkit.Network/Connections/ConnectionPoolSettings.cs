namespace UdpToolkit.Network.Connections
{
    using System;

    public class ConnectionPoolSettings
    {
        public ConnectionPoolSettings(
            TimeSpan connectionTimeout,
            TimeSpan connectionsCleanupFrequency)
        {
            ConnectionTimeout = connectionTimeout;
            ConnectionsCleanupFrequency = connectionsCleanupFrequency;
        }

        public TimeSpan ConnectionTimeout { get; }

        public TimeSpan ConnectionsCleanupFrequency { get; }
    }
}