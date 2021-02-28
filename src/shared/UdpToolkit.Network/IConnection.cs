namespace UdpToolkit.Network
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using UdpToolkit.Network.Channels;

    public interface IConnection
    {
        Guid ConnectionId { get; }

        List<IPEndPoint> IpEndPoints { get; }

        DateTimeOffset LastHeartbeat { get; }

        DateTimeOffset? LastHeartbeatAck { get; }

        DateTimeOffset? LastActivityAt { get; }

        IChannel GetIncomingChannel(ChannelType channelType);

        IChannel GetOutcomingChannel(ChannelType channelType);

        IEnumerable<IChannel> GetChannels();

        void OnHeartbeatAck(
            DateTimeOffset utcNow);

        void OnHeartbeat(
            DateTimeOffset utcNow);

        void OnActivity(
            DateTimeOffset lastActivityAt);

        bool IsExpired();

        TimeSpan GetRtt();

        IPEndPoint GetIp();
    }
}