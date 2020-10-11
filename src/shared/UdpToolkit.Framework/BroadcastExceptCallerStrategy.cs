namespace UdpToolkit.Framework
{
    using UdpToolkit.Network.Packets;
    using UdpToolkit.Network.Queues;

    public sealed class BroadcastExceptCallerStrategy : IBroadcastStrategy
    {
        private readonly IRawRoomManager _rawRoomManager;
        private readonly IAsyncQueue<NetworkPacket> _outputQueue;

        public BroadcastExceptCallerStrategy(
            BroadcastType type,
            IRawRoomManager rawRoomManager,
            IAsyncQueue<NetworkPacket> outputQueue)
        {
            Type = type;
            _rawRoomManager = rawRoomManager;
            _outputQueue = outputQueue;
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
                    var isCaller = peer.PeerId == networkPacket.PeerId;
                    if (isCaller)
                    {
                        return;
                    }

                    peer
                        .GetChannel(channelType: networkPacket.ChannelType)
                        .HandleOutputPacket(networkPacket: networkPacket);

                    _outputQueue.Produce(networkPacket);
                });
        }
    }
}