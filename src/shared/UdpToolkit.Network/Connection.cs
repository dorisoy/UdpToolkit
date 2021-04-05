namespace UdpToolkit.Network
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using UdpToolkit.Network.Channels;

    public class Connection : IConnection
    {
        private readonly IReadOnlyDictionary<ChannelType, IChannel> _inputChannels;
        private readonly IReadOnlyDictionary<ChannelType, IChannel> _outputChannels;

        private Connection(
            Guid connectionId,
            bool keepAlive,
            IPEndPoint ipEndPoint,
            DateTimeOffset lastHeartbeat,
            IReadOnlyDictionary<ChannelType, IChannel> inputChannels,
            IReadOnlyDictionary<ChannelType, IChannel> outputChannels)
        {
            _inputChannels = inputChannels;
            _outputChannels = outputChannels;
            ConnectionId = connectionId;
            Ip = ipEndPoint;
            KeepAlive = keepAlive;
            LastHeartbeat = lastHeartbeat;
        }

        public Guid ConnectionId { get; }

        public bool KeepAlive { get; }

        public IPEndPoint Ip { get; }

        public DateTimeOffset LastHeartbeat { get; private set; }

        public DateTimeOffset? LastHeartbeatAck { get; private set; }

        public static IConnection New(
            Guid connectionId,
            bool keepAlive,
            DateTimeOffset lastHeartbeat,
            IPEndPoint ipEndPoint)
        {
            return new Connection(
                connectionId: connectionId,
                keepAlive: keepAlive,
                lastHeartbeat: lastHeartbeat,
                ipEndPoint: ipEndPoint,
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