namespace UdpToolkit.Network.Contracts.Events
{
    /// <summary>
    /// Raised when inactive connection scan started.
    /// </summary>
    public readonly struct ScanInactiveConnectionsStarted
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ScanInactiveConnectionsStarted"/> struct.
        /// </summary>
        /// <param name="connectionsCount">Connections count.</param>
        public ScanInactiveConnectionsStarted(
            int connectionsCount)
        {
            ConnectionsCount = connectionsCount;
        }

        /// <summary>
        /// Gets started at date time.
        /// </summary>
        public int ConnectionsCount { get; }
    }
}