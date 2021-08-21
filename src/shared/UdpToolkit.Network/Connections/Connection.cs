namespace UdpToolkit.Network.Connections
{
    using System;
    using System.Collections.Generic;
    using UdpToolkit.Network.Contracts.Channels;
    using UdpToolkit.Network.Contracts.Sockets;

    internal sealed class Connection : IConnection
    {
        private readonly IReadOnlyDictionary<byte, IChannel> _inputChannels;
        private readonly IReadOnlyDictionary<byte, IChannel> _outputChannels;

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

        public Guid ConnectionId { get; }

        public bool KeepAlive { get; }

        public IpV4Address IpV4Address { get; }

        public DateTimeOffset LastHeartbeat { get; private set; }

        public DateTimeOffset? LastHeartbeatAck { get; private set; }

        public IChannel GetIncomingChannel(byte channelId) => _inputChannels[channelId];

        public IChannel GetOutcomingChannel(byte channelId) => _outputChannels[channelId];

        public void OnHeartbeatAck(
            DateTimeOffset utcNow)
        {
            LastHeartbeatAck = utcNow;
        }

        public void OnHeartbeat(
            DateTimeOffset utcNow)
        {
            LastHeartbeat = utcNow;
        }

        public TimeSpan GetRtt() => LastHeartbeatAck.HasValue
            ? LastHeartbeatAck.Value - LastHeartbeat
            : TimeSpan.Zero;
    }
}