namespace UdpToolkit
{
    using System;
    using UdpToolkit.Framework.Contracts;
    using UdpToolkit.Network.Channels;
    using UdpToolkit.Network.Contracts.Channels;
    using UdpToolkit.Network.Contracts.Connections;
    using UdpToolkit.Network.Contracts.Sockets;
    using UdpToolkit.Network.Sockets;

    /// <inheritdoc />
    public class NetworkSettings : INetworkSettings
    {
        /// <inheritdoc />
        public int MtuSizeLimit { get; set; } = 1500;

        /// <inheritdoc />
        public int UdpClientBufferSize { get; set; } = 2048;

        /// <inheritdoc />
        public bool AllowIncomingConnections { get; set; } = false;

        /// <inheritdoc />
        public int PollFrequency { get; set; } = 15;

        /// <inheritdoc />
        public TimeSpan ConnectionTimeout { get; set; } = TimeSpan.FromSeconds(30);

        /// <inheritdoc />
        public TimeSpan ConnectionsCleanupFrequency { get; set; } = TimeSpan.FromSeconds(10);

        /// <inheritdoc />
        public TimeSpan ResendTimeout { get; set; } = TimeSpan.FromSeconds(15);

        /// <inheritdoc />
        public ISocketFactory SocketFactory { get; set; } = new NativeSocketFactory();

        /// <inheritdoc />
        public IChannelsFactory ChannelsFactory { get; set; } = new ChannelsFactory();

        /// <inheritdoc />
        public IConnectionIdFactory ConnectionIdFactory { get; set; } = new ConnectionIdFactory();
    }
}