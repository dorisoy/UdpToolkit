namespace UdpToolkit.Framework
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using UdpToolkit.Network.Channels;

    public sealed class Peer
    {
        private readonly IReadOnlyDictionary<ChannelType, IChannel> _channels;
        private readonly Random _random = new Random();

        private Peer(
            Guid peerId,
            List<IPEndPoint> peerIps,
            IReadOnlyDictionary<ChannelType, IChannel> channels)
        {
            PeerId = peerId;
            _channels = channels;
            PeerIps = peerIps;
        }

        public Guid PeerId { get; }

        public List<IPEndPoint> PeerIps { get; }

        public static Peer New(Guid peerId, List<IPEndPoint> peerIps)
        {
            return new Peer(
                peerId: peerId,
                peerIps: peerIps,
                channels: new Dictionary<ChannelType, IChannel>
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

        public IChannel GetChannel(ChannelType channelType) => _channels[channelType];
    }
}
