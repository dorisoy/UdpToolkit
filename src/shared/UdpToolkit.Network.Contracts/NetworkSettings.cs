namespace UdpToolkit.Network.Contracts
{
    using System;
    using UdpToolkit.Network.Contracts.Channels;
    using UdpToolkit.Network.Contracts.Sockets;

    public class NetworkSettings
    {
        public int MtuSizeLimit { get; set; } = 1500;

        public int UdpClientBufferSize { get; set; } = 2048;

        public TimeSpan ConnectionTimeout { get; set; } = TimeSpan.FromSeconds(30);

        public TimeSpan ConnectionsCleanupFrequency { get; set; } = TimeSpan.FromSeconds(10);

        public TimeSpan ResendTimeout { get; set; } = TimeSpan.FromSeconds(30);

        public ISocketFactory SocketFactory { get; set; }

        public IChannelsFactory ChannelsFactory { get; set; }
    }
}