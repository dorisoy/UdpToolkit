namespace UdpToolkit.Framework
{
    using UdpToolkit.Network.Packets;
    using UdpToolkit.Network.Queues;

    public sealed class BroadcastServerStrategy : IBroadcastStrategy
    {
        private readonly IAsyncQueue<NetworkPacket> _outputQueue;
        private readonly IRawPeerManager _rawPeerManager;

        public BroadcastServerStrategy(
            BroadcastType type,
            IAsyncQueue<NetworkPacket> outputQueue,
            IRawPeerManager rawPeerManager)
        {
            _outputQueue = outputQueue;
            _rawPeerManager = rawPeerManager;
            Type = type;
        }

        public BroadcastType Type { get; }

        public void Execute(
            ushort roomId,
            NetworkPacket networkPacket)
        {
            _rawPeerManager
                .GetPeer(networkPacket.PeerId)
                .GetChannel(networkPacket.ChannelType)
                .HandleOutputPacket(networkPacket);

            _outputQueue.Produce(@event: networkPacket);
        }
    }
}