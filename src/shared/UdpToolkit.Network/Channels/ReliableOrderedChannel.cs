namespace UdpToolkit.Network.Channels
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using UdpToolkit.Network.Packets;
    using UdpToolkit.Network.Queues;

    [ExcludeFromCodeCoverage]
    public sealed class ReliableOrderedChannel : IChannel
    {
        public ChannelResult TryHandleInputPacket(NetworkPacket networkPacket)
        {
            throw new System.NotImplementedException();
        }

        public NetworkPacket TryHandleOutputPacket(NetworkPacket networkPacket)
        {
            throw new System.NotImplementedException();
        }

        public NetworkPacket HandleAck(
            NetworkPacket networkPacket)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<NetworkPacket> GetPendingPackets()
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<NetworkPacket> ToResend()
        {
            return Enumerable.Empty<NetworkPacket>();
        }

        public void Flush()
        {
            throw new System.NotImplementedException();
        }
    }
}