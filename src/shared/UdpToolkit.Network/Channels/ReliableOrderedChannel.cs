namespace UdpToolkit.Network.Channels
{
    using System.Collections.Generic;
    using UdpToolkit.Network.Packets;
    using UdpToolkit.Network.Queues;

    public sealed class ReliableOrderedChannel : IChannel
    {
        public ChannelResult TryHandleInputPacket(NetworkPacket networkPacket)
        {
            throw new System.NotImplementedException();
        }

        public NetworkPacket? TryHandleOutputPacket(NetworkPacket networkPacket)
        {
            throw new System.NotImplementedException();
        }

        public NetworkPacket? HandleAck(
            NetworkPacket networkPacket)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<NetworkPacket> GetPendingPackets()
        {
            throw new System.NotImplementedException();
        }

        public void Resend(
            IAsyncQueue<NetworkPacket> outputQueue)
        {
            throw new System.NotImplementedException();
        }

        public void Flush()
        {
            throw new System.NotImplementedException();
        }
    }
}