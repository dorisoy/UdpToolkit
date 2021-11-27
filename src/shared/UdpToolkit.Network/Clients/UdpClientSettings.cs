namespace UdpToolkit.Network.Clients
{
    using System;
    using System.Buffers;
    using UdpToolkit.Network.Contracts.Channels;
    using UdpToolkit.Network.Contracts.Sockets;

    /// <summary>
    /// UdpClient settings.
    /// </summary>
    public sealed class UdpClientSettings
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UdpClientSettings"/> class.
        /// </summary>
        /// <param name="mtuSizeLimit">MTU size limit.</param>
        /// <param name="udpClientBufferSize">Size of buffer for UDP client socket.</param>
        /// <param name="pollFrequency">Poll frequency.</param>
        /// <param name="allowIncomingConnections">Ability to accept incoming connections.</param>
        /// <param name="resendTimeout">Timeout for packets in resend queue.</param>
        /// <param name="channelsFactory">Instance of channelsFactory.</param>
        /// <param name="socketFactory">Instance of socketFactory.</param>
        /// <param name="packetsPoolSize">Size of packets pool.</param>
        /// <param name="packetsBufferPoolSize">Size of packets buffers pool.</param>
        /// <param name="headersBuffersPoolSize">Size of headers buffers pool.</param>
        /// <param name="arrayPool">ArrayPool.</param>
        public UdpClientSettings(
            int mtuSizeLimit,
            int udpClientBufferSize,
            int pollFrequency,
            bool allowIncomingConnections,
            TimeSpan resendTimeout,
            IChannelsFactory channelsFactory,
            ISocketFactory socketFactory,
            int packetsPoolSize,
            int packetsBufferPoolSize,
            int headersBuffersPoolSize,
            ArrayPool<byte> arrayPool)
        {
            MtuSizeLimit = mtuSizeLimit;
            UdpClientBufferSize = udpClientBufferSize;
            PollFrequency = pollFrequency;
            AllowIncomingConnections = allowIncomingConnections;
            ResendTimeout = resendTimeout;
            ChannelsFactory = channelsFactory;
            SocketFactory = socketFactory;
            PacketsPoolSize = packetsPoolSize;
            PacketsBufferPoolSize = packetsBufferPoolSize;
            HeadersBuffersPoolSize = headersBuffersPoolSize;
            ArrayPool = arrayPool;
        }

        /// <summary>
        /// Gets a value of MTU size limit.
        /// </summary>
        public int MtuSizeLimit { get; }

        /// <summary>
        /// Gets the size of buffer for UDP client socket.
        /// </summary>
        public int UdpClientBufferSize { get; }

        /// <summary>
        /// Gets a value of poll frequency.
        /// </summary>
        public int PollFrequency { get; }

        /// <summary>
        /// Gets a value indicating whether ability to accept incoming connections.
        /// </summary>
        public bool AllowIncomingConnections { get; }

        /// <summary>
        /// Gets resend timeout.
        /// </summary>
        public TimeSpan ResendTimeout { get; }

        /// <summary>
        /// Gets ChannelsFactory instance.
        /// </summary>
        public IChannelsFactory ChannelsFactory { get; }

        /// <summary>
        /// Gets SocketFactory instance.
        /// </summary>
        public ISocketFactory SocketFactory { get; }

        /// <summary>
        /// Gets the size of packets pool.
        /// </summary>
        public int PacketsPoolSize { get; }

        /// <summary>
        /// Gets the size of packets buffers pool.
        /// </summary>
        public int PacketsBufferPoolSize { get; }

        /// <summary>
        /// Gets the size of headers buffers pool.
        /// </summary>
        public int HeadersBuffersPoolSize { get; }

        /// <summary>
        /// Gets the size of headers buffers pool.
        /// </summary>
        public ArrayPool<byte> ArrayPool { get; }
    }
}