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

        DateTimeOffset LastPing { get; }

        DateTimeOffset LastPong { get; }

        DateTimeOffset? LastActivityAt { get; }

        IChannel GetIncomingChannel(ChannelType channelType);

        IChannel GetOutcomingChannel(ChannelType channelType);

        IEnumerable<IChannel> GetChannels();

        void OnPing(
            DateTimeOffset onPingReceive);

        void OnPong(
            DateTimeOffset onPongReceive);

        void OnActivity(
            DateTimeOffset lastActivityAt);

        bool IsExpired();

        TimeSpan GetRtt();

        IPEndPoint GetRandomIp();
    }
}