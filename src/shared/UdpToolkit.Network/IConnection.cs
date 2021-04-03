namespace UdpToolkit.Network
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using UdpToolkit.Network.Channels;

    public interface IConnection
    {
        Guid ConnectionId { get; }

        bool KeepAlive { get; }

        List<IPEndPoint> IpEndPoints { get; }

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

        IPEndPoint GetIp();
    }
}