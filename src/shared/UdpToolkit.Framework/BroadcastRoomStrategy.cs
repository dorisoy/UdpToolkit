namespace UdpToolkit.Framework
{
    using UdpToolkit.Network.Packets;
    using UdpToolkit.Network.Queues;

    public sealed class BroadcastRoomStrategy : IBroadcastStrategy
    {
        private readonly IRawRoomManager _rawRoomManager;
        private readonly IAsyncQueue<NetworkPacket> _outputQueue;

        public BroadcastRoomStrategy(
            IRawRoomManager rawRoomManager,
            IAsyncQueue<NetworkPacket> outputQueue,
            BroadcastType type)
        {
            _rawRoomManager = rawRoomManager;
            _outputQueue = outputQueue;
            Type = type;
        }

        public BroadcastType Type { get; }

        public void Execute(
            ushort roomId,
            NetworkPacket networkPacket)
        {
            _rawRoomManager.Apply(
                roomId: roomId,
                condition: (peer) => true,
                action: (peer) =>
                {
                    var newPacket = networkPacket
                        .Clone();

                    peer
                        .GetChannel(channelType: newPacket.ChannelType)
                        .HandleOutputPacket(networkPacket: newPacket);

                    var packet = newPacket
                        .SetIpEndPoint(peer.GetRandomIp())
                        .SetPeerId(peer.PeerId);

                    _outputQueue.Produce(packet);
                });
        }
    }
}