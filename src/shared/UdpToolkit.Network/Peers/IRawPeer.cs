namespace UdpToolkit.Network.Peers
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using UdpToolkit.Network.Channels;

    public interface IRawPeer
    {
        Guid PeerId { get; }

        IChannel GetIncomingChannel(ChannelType channelType);

        IChannel GetOutcomingChannel(ChannelType channelType);

        IEnumerable<IChannel> GetChannels();

        IPEndPoint GetRandomIp();
    }
}