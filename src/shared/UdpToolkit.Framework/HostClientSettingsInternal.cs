namespace UdpToolkit.Framework
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
        /// <param name="resendDelayMs">Resend delay in ms.</param>
        /// <param name="connectionTimeout">Timeout for connection.</param>
        /// <param name="serverIpV4">Remote host ip address.</param>
        public HostClientSettingsInternal(
            int? resendDelayMs,
            TimeSpan connectionTimeout,
            IpV4Address serverIpV4)
        {
            ResendDelayMs = resendDelayMs;
            ConnectionTimeout = connectionTimeout;
            ServerIpV4 = serverIpV4;
        }

        /// <summary>
        /// Gets delay for resending packets to remote host.
        /// </summary>
        public int? ResendDelayMs { get; }

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