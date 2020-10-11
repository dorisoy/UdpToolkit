namespace UdpToolkit.Framework
{
    using System;
    using UdpToolkit.Core;
    using UdpToolkit.Network.Channels;
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
                    var isCaller = peer.PeerId == networkPacket.PeerId;
                    if (isCaller)
                    {
                        var ackPacket = peer
                            .GetChannel(channelType: networkPacket.ChannelType)
                            .GetAck(networkPacket, peer.GetRandomIp());

                        _outputQueue.Produce(ackPacket);
                    }
                    else
                    {
                        peer
                            .GetChannel(channelType: networkPacket.ChannelType)
                            .HandleOutputPacket(networkPacket: networkPacket);

                        _outputQueue.Produce(networkPacket);
                    }
                });
        }
    }
}