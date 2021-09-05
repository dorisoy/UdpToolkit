namespace UdpToolkit.Network.Connections
{
    using System;
    using System.Collections.Generic;
    using UdpToolkit.Network.Contracts.Channels;
    using UdpToolkit.Network.Contracts.Sockets;

    /// <inheritdoc />
    internal sealed class Connection : IConnection
    {
        private readonly IReadOnlyDictionary<byte, IChannel> _inputChannels;
        private readonly IReadOnlyDictionary<byte, IChannel> _outputChannels;

        /// <summary>
        /// Initializes a new instance of the <see cref="Connection"/> class.
        /// </summary>
        /// <param name="connectionId">Connection identifier.</param>
        /// <param name="keepAlive">Flag indicating whether needs to remove the connection from the pool on cleanup scan.</param>
        /// <param name="ipAddress">Ip address of connection.</param>
        /// <param name="lastHeartbeat">Last heartbeat (init value).</param>
        /// <param name="inputChannels">List of input channels.</param>
        /// <param name="outputChannels">List of output channels.</param>
        internal Connection(
            Guid connectionId,
            bool keepAlive,
            IpV4Address ipAddress,
            DateTimeOffset lastHeartbeat,
            IReadOnlyDictionary<byte, IChannel> inputChannels,
            IReadOnlyDictionary<byte, IChannel> outputChannels)
        {
            _inputChannels = inputChannels;
            _outputChannels = outputChannels;
            ConnectionId = connectionId;
            IpV4Address = ipAddress;
            KeepAlive = keepAlive;
            LastHeartbeat = lastHeartbeat;
        }

        /// <inheritdoc />
        public Guid ConnectionId { get; }

        /// <inheritdoc />
        public bool KeepAlive { get; }

        /// <inheritdoc />
        public IpV4Address IpV4Address { get; }

        /// <inheritdoc />
        public DateTimeOffset LastHeartbeat { get; private set; }

        private DateTimeOffset? LastHeartbeatAck { get; set; }

        /// <inheritdoc />
        public IChannel GetIncomingChannel(byte channelId) => _inputChannels[channelId];

        /// <inheritdoc />
        public IChannel GetOutgoingChannel(byte channelId) => _outputChannels[channelId];

        /// <inheritdoc />
        public void OnHeartbeatAck(
            DateTimeOffset utcNow)
        {
            LastHeartbeatAck = utcNow;
        }

        /// <inheritdoc />
        public void OnHeartbeat(
            DateTimeOffset utcNow)
        {
            LastHeartbeat = utcNow;
        }

        /// <inheritdoc />
        public TimeSpan GetRtt() => LastHeartbeatAck.HasValue
            ? LastHeartbeatAck.Value - LastHeartbeat
            : TimeSpan.Zero;
    }
}