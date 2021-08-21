namespace UdpToolkit.Network.Clients
{
    using System;
    using UdpToolkit.Network.Contracts.Channels;
    using UdpToolkit.Network.Contracts.Sockets;

    public sealed class UdpClientSettings
    {
        public UdpClientSettings(
            int mtuSizeLimit,
            int udpClientBufferSize,
            int pollFrequency,
            bool allowIncomingConnections,
            TimeSpan resendTimeout,
            IChannelsFactory channelsFactory,
            ISocketFactory socketFactory)
        {
            MtuSizeLimit = mtuSizeLimit;
            UdpClientBufferSize = udpClientBufferSize;
            PollFrequency = pollFrequency;
            AllowIncomingConnections = allowIncomingConnections;
            ResendTimeout = resendTimeout;
            ChannelsFactory = channelsFactory;
            SocketFactory = socketFactory;
        }

        public int MtuSizeLimit { get; }

        public int UdpClientBufferSize { get; }

        public int PollFrequency { get; }

        public bool AllowIncomingConnections { get; }

        public TimeSpan ResendTimeout { get; }

        public IChannelsFactory ChannelsFactory { get; }

        public ISocketFactory SocketFactory { get; }
    }
}