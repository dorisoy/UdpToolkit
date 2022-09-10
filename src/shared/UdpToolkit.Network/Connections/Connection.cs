namespace UdpToolkit.Network.Connections
{
    using System;
    using System.Collections.Generic;
    using UdpToolkit.Network.Contracts.Channels;
    using UdpToolkit.Network.Contracts.Connections;
    using UdpToolkit.Network.Contracts.Packets;
    using UdpToolkit.Network.Contracts.Sockets;

    /// <inheritdoc />
    internal sealed class Connection : IConnection
    {
        private readonly IReadOnlyDictionary<byte, IChannel> _inputChannelsMap;
        private readonly IReadOnlyDictionary<byte, IChannel> _outputChannelsMap;

        /// <summary>
        /// Initializes a new instance of the <see cref="Connection"/> class.
        /// </summary>
        /// <param name="connectionId">Connection identifier.</param>
        /// <param name="routingKey">Routing key.</param>
        /// <param name="keepAlive">Flag indicating whether needs to remove the connection from the pool on cleanup scan.</param>
        /// <param name="ipAddress">Ip address of connection.</param>
        /// <param name="createdAt">Connection creation date.</param>
        /// <param name="inputChannelsMap">Map of input channels.</param>
        /// <param name="outputChannelsMap">Map of output channels.</param>
        internal Connection(
            Guid connectionId,
            Guid routingKey,
            bool keepAlive,
            IpV4Address ipAddress,
            DateTimeOffset createdAt,
            IReadOnlyDictionary<byte, IChannel> inputChannelsMap,
            IReadOnlyDictionary<byte, IChannel> outputChannelsMap)
        {
            _inputChannelsMap = inputChannelsMap;
            _outputChannelsMap = outputChannelsMap;
            RoutingKey = routingKey;
            ConnectionId = connectionId;
            IpV4Address = ipAddress;
            KeepAlive = keepAlive;
            LastActivityAt = createdAt;
            PendingPackets = new List<PendingPacket>(100); // TODO move to config
        }

        /// <inheritdoc />
        public IList<PendingPacket> PendingPackets { get; }

        /// <inheritdoc />
        public Guid ConnectionId { get; }

        /// <inheritdoc />
        public Guid RoutingKey { get; }

        /// <inheritdoc />
        public bool KeepAlive { get; }

        /// <inheritdoc />
        public IpV4Address IpV4Address { get; }

        /// <inheritdoc />
        public DateTimeOffset LastActivityAt { get; private set; }

        private DateTimeOffset LastPing { get; set; }

        private DateTimeOffset? LastPingAck { get; set; }

        /// <inheritdoc />
        public bool GetIncomingChannel(byte channelId, out IChannel channel) => _inputChannelsMap.TryGetValue(channelId, out channel);

        /// <inheritdoc />
        public bool GetOutgoingChannel(byte channelId, out IChannel channel) => _outputChannelsMap.TryGetValue(channelId, out channel);

        /// <inheritdoc />
        public void OnPingAck(
            DateTimeOffset utcNow)
        {
            LastPingAck = utcNow;
        }

        /// <inheritdoc />
        public void OnPing(
            DateTimeOffset utcNow)
        {
            LastPing = utcNow;
        }

        /// <inheritdoc />
        public void OnConnectionActivity(
            DateTimeOffset utcNow)
        {
            LastActivityAt = utcNow;
        }

        /// <inheritdoc />
        public double GetRtt()
        {
            var diff = LastPingAck.HasValue
                ? LastPingAck.Value - LastPing
                : default;

            return diff.TotalMilliseconds;
        }
    }
}