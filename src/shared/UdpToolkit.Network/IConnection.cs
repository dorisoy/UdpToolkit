namespace UdpToolkit.Network
{
    using System;
    using UdpToolkit.Network.Channels;
    using UdpToolkit.Network.Sockets;

    public interface IConnection
    {
        Guid ConnectionId { get; }

        bool KeepAlive { get; }

        IpV4Address IpAddress { get; }

        DateTimeOffset LastHeartbeat { get; }

        DateTimeOffset? LastHeartbeatAck { get; }

        IChannel GetIncomingChannel(
            ChannelType channelType);

        IChannel GetOutcomingChannel(
            ChannelType channelType);

        void OnHeartbeatAck(
            DateTimeOffset utcNow);

        void OnHeartbeat(
            DateTimeOffset utcNow);

        TimeSpan GetRtt();
    }
}