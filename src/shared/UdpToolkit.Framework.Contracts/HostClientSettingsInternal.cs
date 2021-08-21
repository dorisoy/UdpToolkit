namespace UdpToolkit.Framework.Contracts
{
    using System;
    using UdpToolkit.Network.Contracts.Sockets;

    public sealed class HostClientSettingsInternal
    {
        public HostClientSettingsInternal(
            int? heartbeatDelayMs,
            TimeSpan connectionTimeout,
            IpV4Address serverIpV4)
        {
            HeartbeatDelayMs = heartbeatDelayMs;
            ConnectionTimeout = connectionTimeout;
            ServerIpV4 = serverIpV4;
        }

        public int? HeartbeatDelayMs { get; }

        public TimeSpan ConnectionTimeout { get; }

        public IpV4Address ServerIpV4 { get; }
    }
}