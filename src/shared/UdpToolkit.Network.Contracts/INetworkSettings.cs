namespace UdpToolkit.Network.Contracts
{
    using System;
    using System.Buffers;
    using UdpToolkit.Network.Contracts.Channels;
    using UdpToolkit.Network.Contracts.Events;
    using UdpToolkit.Network.Contracts.Sockets;

    /// <summary>
    /// Network settings.
    /// </summary>
    public interface INetworkSettings
    {
        /// <summary>
        /// Gets or sets MTU size.
        /// </summary>
        /// <remarks>
        /// The temporary solution you set this value manually or detected by an external tool, all packets over this value will be dropped before sending to another host.
        /// </remarks>
        int MtuSizeLimit { get; set; }

        /// <summary>
        /// Gets or sets buffer size for socket in UDP client.
        /// </summary>
        int UdpClientBufferSize { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the host's ability to accept incoming connections.
        /// </summary>
        /// <remarks>
        /// Use cases:
        /// 1) Server - should be true.
        /// 2) Client - should be false.
        /// 3) MasterClient(p2p) - should be true.
        /// </remarks>
        bool AllowIncomingConnections { get; set; }

        /// <summary>
        /// Gets or sets polling frequency for socket in UDP client.
        /// </summary>
        int PollFrequency { get; set; }

        /// <summary>
        /// Gets or sets the timeout for connection to the remote host.
        /// </summary>
        TimeSpan ConnectionTimeout { get; set; }

        /// <summary>
        /// Gets or sets the frequency for searching and removing inactive connections.
        /// </summary>
        TimeSpan ConnectionsCleanupFrequency { get; set; }

        /// <summary>
        /// Gets or sets the timeout for packets sent over reliable channels.
        /// </summary>
        TimeSpan ResendTimeout { get; set; }

        /// <summary>
        /// Gets or sets SocketFactory instance, for providing socket instance for UDP client.
        /// </summary>
        ISocketFactory SocketFactory { get; set; }

        /// <summary>
        /// Gets or sets ChannelFactory instance, for list providing of available channels.
        /// </summary>
        IChannelsFactory ChannelsFactory { get; set; }

        /// <summary>
        /// Gets or sets the size of packets pool.
        /// </summary>
        int PacketsPoolSize { get; set; }

        /// <summary>
        /// Gets or sets the size of packets buffers pool.
        /// </summary>
        int PacketsBufferPoolSize { get; set; }

        /// <summary>
        /// Gets or sets the size of headers buffers pool.
        /// </summary>
        int HeadersBuffersPoolSize { get; set; }

        /// <summary>
        /// Gets or sets array pool instance.
        /// </summary>
        ArrayPool<byte> ArrayPool { get; set; }

        /// <summary>
        /// Gets or sets event reporter instance.
        /// </summary>
        INetworkEventReporter NetworkEventReporter { get; set; }
    }
}