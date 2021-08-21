namespace UdpToolkit.Framework.Contracts
{
    using System;
    using UdpToolkit.Network.Contracts.Channels;
    using UdpToolkit.Network.Contracts.Connections;
    using UdpToolkit.Network.Contracts.Sockets;

    public interface INetworkSettings
    {
        public int MtuSizeLimit { get; set; }

        public int UdpClientBufferSize { get; set; }

        public bool AllowIncomingConnections { get; set; }

        public int PollFrequency { get; set; }

        public TimeSpan ConnectionTimeout { get; set; }

        public TimeSpan ConnectionsCleanupFrequency { get; set; }

        public TimeSpan ResendTimeout { get; set; }

        public ISocketFactory SocketFactory { get; set; }

        public IChannelsFactory ChannelsFactory { get; set; }

        public IConnectionIdFactory ConnectionIdFactory { get; set; }
    }
}