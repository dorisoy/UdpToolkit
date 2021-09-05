namespace UdpToolkit.Framework.Contracts
{
    using System;
    using UdpToolkit.Network.Contracts.Sockets;

    /// <summary>
    /// Internal client settings.
    /// </summary>
    public sealed class HostClientSettingsInternal
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HostClientSettingsInternal"/> class.
        /// </summary>
        /// <param name="heartbeatDelayMs">Heartbeat delay in ms.</param>
        /// <param name="connectionTimeout">Timeout for connection.</param>
        /// <param name="serverIpV4">Remote host ip address.</param>
        public HostClientSettingsInternal(
            int? heartbeatDelayMs,
            TimeSpan connectionTimeout,
            IpV4Address serverIpV4)
        {
            HeartbeatDelayMs = heartbeatDelayMs;
            ConnectionTimeout = connectionTimeout;
            ServerIpV4 = serverIpV4;
        }

        /// <summary>
        /// Gets delay for sending heartbeat to remote host.
        /// </summary>
        /// <remarks>
        /// Needed only for client's.
        /// 1) Heartbeat initiate resending of reliable packages on the client-side and resending acknowledge packets on the server-side.
        /// 2) Heartbeat measures the round trip time between client and server host.
        /// 3) Pass null value for disabling Heartbeats, use this setting only on localhost.
        /// </remarks>
        public int? HeartbeatDelayMs { get; }

        /// <summary>
        /// Gets timeout for connection.
        /// </summary>
        public TimeSpan ConnectionTimeout { get; }

        /// <summary>
        /// Gets remote host ip address.
        /// </summary>
        public IpV4Address ServerIpV4 { get; }
    }
}