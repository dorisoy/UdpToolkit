namespace UdpToolkit
{
    using System;
    using System.Buffers;
    using UdpToolkit.Network.Channels;
    using UdpToolkit.Network.Contracts;
    using UdpToolkit.Network.Contracts.Channels;
    using UdpToolkit.Network.Contracts.Events;
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
        public int PacketsPoolSize { get; set; } = 100;

        /// <inheritdoc />
        public int PacketsBufferPoolSize { get; set; } = 100;

        /// <inheritdoc />
        public int HeadersBuffersPoolSize { get; set; } = 100;

        /// <inheritdoc />
        public ArrayPool<byte> ArrayPool { get; set; } = ArrayPool<byte>.Shared;

        /// <inheritdoc />
        public INetworkEventReporter NetworkEventReporter { get; set; } = new DefaultNetworkEventReporter();
    }
}