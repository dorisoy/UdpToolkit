namespace UdpToolkit.Framework.Contracts.Settings
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Host client settings.
    /// </summary>
    public class HostClientSettings
    {
        /// <summary>
        /// Gets or sets timeout for connection.
        /// </summary>
        public TimeSpan ConnectionTimeout { get; set; } = TimeSpan.FromSeconds(15);

        /// <summary>
        /// Gets or sets remote host ip address in string representation.
        /// </summary>
        public string ServerHost { get; set; } = "127.0.0.1";

        /// <summary>
        /// Gets or sets remote host ports for receive packets.
        /// </summary>
        public IEnumerable<int> ServerPorts { get; set; } = Array.Empty<int>();

        /// <summary>
        /// Gets or sets delay for sending heartbeat to remote host.
        /// </summary>
        /// <remarks>
        /// Needed only for client's.
        /// 1) Heartbeat initiate resending of reliable packages on the client-side and resending acknowledge packets on the server-side.
        /// 2) Heartbeat measures the round trip time between client and server host.
        /// 3) Pass null value for disabling Heartbeats, use this setting only on localhost.
        /// </remarks>
        public int? HeartbeatDelayInMs { get; set; } = 1000;
    }
}