namespace UdpToolkit.Framework
{
    using UdpToolkit.Network.Channels;
    using UdpToolkit.Network.Packets;
    using UdpToolkit.Network.Queues;

    public sealed class BroadcastCallerStrategy : IBroadcastStrategy
    {
        private readonly IRawPeerManager _rawPeerManager;
        private readonly IAsyncQueue<NetworkPacket> _outputQueue;

        public BroadcastCallerStrategy(
            BroadcastType type,
            IRawPeerManager rawPeerManager,
            IAsyncQueue<NetworkPacket> outputQueue)
        {
            Type = type;
            _rawPeerManager = rawPeerManager;
            _outputQueue = outputQueue;
        }

        public BroadcastType Type { get; }

        public void Execute(
            ushort roomId,
            NetworkPacket networkPacket)
        {
            var caller = _rawPeerManager
                .GetPeer(peerId: networkPacket.PeerId);

            if (networkPacket.ChannelType == ChannelType.ReliableUdp)
            {
                networkPacket = caller
                    .GetChannel(networkPacket.ChannelType)
                    .GetAck(networkPacket: networkPacket, ipEndPoint: caller.GetRandomIp());
            }

            _rawPeerManager
                .GetPeer(peerId: networkPacket.PeerId)
                .GetChannel(networkPacket.ChannelType)
                .HandleOutputPacket(networkPacket: networkPacket);

            _outputQueue.Produce(@event: networkPacket);
        }
    }
}