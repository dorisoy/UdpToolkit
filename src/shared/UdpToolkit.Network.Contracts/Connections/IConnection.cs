namespace UdpToolkit.Network.Contracts.Connections
{
    using System;
    using UdpToolkit.Network.Contracts.Channels;
    using UdpToolkit.Network.Contracts.Sockets;

    public interface IConnection
    {
        Guid ConnectionId { get; }

        bool KeepAlive { get; }

        IpV4Address IpAddress { get; }

        DateTimeOffset LastHeartbeat { get; }

        DateTimeOffset? LastHeartbeatAck { get; }

        IChannel GetIncomingChannel(
            byte channelId);

        IChannel GetOutcomingChannel(
            byte channelId);

        void OnHeartbeatAck(
            DateTimeOffset utcNow);

        void OnHeartbeat(
            DateTimeOffset utcNow);

        TimeSpan GetRtt();
    }
}