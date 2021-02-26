namespace UdpToolkit.Network
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using UdpToolkit.Network.Channels;

    public class Connection : IConnection
    {
        private readonly Random _random = new Random();
        private readonly IReadOnlyDictionary<ChannelType, IChannel> _inputChannels;
        private readonly IReadOnlyDictionary<ChannelType, IChannel> _outputChannels;
        private readonly TimeSpan _connectionTimeout;

        private Connection(
            Guid connectionId,
            List<IPEndPoint> ipEndPoints,
            TimeSpan connectionTimeout,
            IReadOnlyDictionary<ChannelType, IChannel> inputChannels,
            IReadOnlyDictionary<ChannelType, IChannel> outputChannels)
        {
            _inputChannels = inputChannels;
            _outputChannels = outputChannels;
            this._connectionTimeout = connectionTimeout;
            ConnectionId = connectionId;
            IpEndPoints = ipEndPoints;
        }

        public Guid ConnectionId { get; }

        public List<IPEndPoint> IpEndPoints { get; }

        public DateTimeOffset LastPing { get; private set; }

        public DateTimeOffset LastPong { get; private set; }

        public DateTimeOffset? LastActivityAt { get; private set; }

        public static IConnection New(
            Guid connectionId,
            List<IPEndPoint> ipEndPoints,
            TimeSpan connectionTimeout)
        {
            return new Connection(
                connectionId: connectionId,
                ipEndPoints: ipEndPoints,
                connectionTimeout: connectionTimeout,
                outputChannels: new Dictionary<ChannelType, IChannel>
                {
                    [ChannelType.Udp] = new RawUdpChannel(),
                    [ChannelType.ReliableUdp] = new ReliableChannel(windowSize: 1024),
                    [ChannelType.ReliableOrderedUdp] = new ReliableOrderedChannel(),
                    [ChannelType.Sequenced] = new SequencedChannel(),
                },
                inputChannels: new Dictionary<ChannelType, IChannel>
                {
                    [ChannelType.Udp] = new RawUdpChannel(),
                    [ChannelType.ReliableUdp] = new ReliableChannel(windowSize: 1024),
                    [ChannelType.ReliableOrderedUdp] = new ReliableOrderedChannel(),
                    [ChannelType.Sequenced] = new SequencedChannel(),
                });
        }

        public IChannel GetIncomingChannel(ChannelType channelType) => _inputChannels[channelType];

        public IChannel GetOutcomingChannel(ChannelType channelType) => _outputChannels[channelType];

        public IEnumerable<IChannel> GetChannels() => _outputChannels.Values;

        public void OnPing(
            DateTimeOffset onPingReceive)
        {
            LastPing = onPingReceive;
        }

        public void OnPong(
            DateTimeOffset onPongReceive)
        {
            LastPong = onPongReceive;
        }

        public void OnActivity(
            DateTimeOffset lastActivityAt)
        {
            LastActivityAt = lastActivityAt;
        }

        public bool IsExpired() => DateTimeOffset.UtcNow - LastActivityAt > _connectionTimeout;

        public TimeSpan GetRtt() => LastPong - LastPing;

        public IPEndPoint GetRandomIp()
        {
            return IpEndPoints[_random.Next(0, IpEndPoints.Count - 1)];
        }
    }
}