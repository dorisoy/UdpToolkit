namespace UdpToolkit.Network.Connections
{
    using System;
    using UdpToolkit.Network.Contracts.Channels;
    using UdpToolkit.Network.Contracts.Sockets;

    internal interface IConnection
    {
        Guid ConnectionId { get; }

        bool KeepAlive { get; }

        IpV4Address IpV4Address { get; }

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