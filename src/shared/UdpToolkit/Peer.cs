namespace UdpToolkit
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading;
    using UdpToolkit.Core;
    using UdpToolkit.Network.Channels;
    using UdpToolkit.Network.Packets;
    using UdpToolkit.Network.Peers;
    using UdpToolkit.Network.Queues;

    public sealed class Peer : IPeer, IRawPeer
    {
        private readonly IReadOnlyDictionary<ChannelType, IChannel> _inputChannels;
        private readonly IReadOnlyDictionary<ChannelType, IChannel> _outputChannels;
        private readonly TimeSpan _inactivityTimeout;
        private readonly Random _random = new Random();
        private int _roomId;

        private Peer(
            Guid peerId,
            List<IPEndPoint> peerIps,
            IReadOnlyDictionary<ChannelType, IChannel> inputChannels,
            IReadOnlyDictionary<ChannelType, IChannel> outputChannels,
            TimeSpan inactivityTimeout)
        {
            PeerId = peerId;
            _inputChannels = inputChannels;
            _outputChannels = outputChannels;
            _inactivityTimeout = inactivityTimeout;
            PeerIps = peerIps;
        }

        public Guid PeerId { get; }

        public List<IPEndPoint> PeerIps { get; }

        public DateTimeOffset LastPing { get; private set; }

        public DateTimeOffset LastPong { get; private set; }

        public DateTimeOffset? LastActivityAt { get; private set; }

        public static Peer New(
            Guid peerId,
            List<IPEndPoint> peerIps,
            TimeSpan inactivityTimeout)
        {
            return new Peer(
                peerId: peerId,
                peerIps: peerIps,
                inactivityTimeout: inactivityTimeout,
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

        public IPEndPoint GetRandomIp()
        {
            return PeerIps[_random.Next(0, PeerIps.Count - 1)];
        }

        public int GetRoomId() => _roomId;

        public void SetRoomId(
            int roomId)
        {
            _roomId = roomId;
        }

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

        public bool IsExpired() => DateTimeOffset.UtcNow - LastActivityAt > _inactivityTimeout;

        public TimeSpan GetRtt() => LastPong - LastPing;

        public IChannel GetIncomingChannel(ChannelType channelType) => _inputChannels[channelType];

        public IChannel GetOutcomingChannel(ChannelType channelType) => _outputChannels[channelType];

        public IEnumerable<IChannel> GetChannels() => _outputChannels.Values;
    }
}
