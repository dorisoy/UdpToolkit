namespace UdpToolkit
{
    using System;
    using UdpToolkit.Framework.Contracts;
    using UdpToolkit.Network.Channels;
    using UdpToolkit.Network.Contracts.Channels;
    using UdpToolkit.Network.Contracts.Connections;
    using UdpToolkit.Network.Contracts.Sockets;
    using UdpToolkit.Network.Sockets;

    public class NetworkSettings : INetworkSettings
    {
        public int MtuSizeLimit { get; set; } = 1500;

        public int UdpClientBufferSize { get; set; } = 2048;

        public bool AllowIncomingConnections { get; set; } = false;

        public int PollFrequency { get; set; } = 15;

        public TimeSpan ConnectionTimeout { get; set; } = TimeSpan.FromSeconds(30);

        public TimeSpan ConnectionsCleanupFrequency { get; set; } = TimeSpan.FromSeconds(10);

        public TimeSpan ResendTimeout { get; set; } = TimeSpan.FromSeconds(15);

        public ISocketFactory SocketFactory { get; set; } = new NativeSocketFactory();

        public IChannelsFactory ChannelsFactory { get; set; } = new ChannelsFactory();

        public IConnectionIdFactory ConnectionIdFactory { get; set; } = new ConnectionIdFactory();
    }
}