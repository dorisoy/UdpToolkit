namespace UdpToolkit.Framework.Contracts
{
    using System;
    using UdpToolkit.Network.Contracts.Sockets;

    public sealed class HostClientSettingsInternal
    {
        public HostClientSettingsInternal(
            int? heartbeatDelayMs,
            TimeSpan connectionTimeout,
            Guid connectionId,
            IpV4Address serverIpV4)
        {
            HeartbeatDelayMs = heartbeatDelayMs;
            ConnectionTimeout = connectionTimeout;
            ConnectionId = connectionId;
            ServerIpV4 = serverIpV4;
        }

        public int? HeartbeatDelayMs { get; }

        public TimeSpan ConnectionTimeout { get; }

        public Guid ConnectionId { get; }

        public IpV4Address ServerIpV4 { get; }
    }
}