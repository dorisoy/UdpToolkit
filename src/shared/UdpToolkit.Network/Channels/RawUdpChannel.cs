namespace UdpToolkit.Network.Channels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UdpToolkit.Network.Packets;
    using UdpToolkit.Network.Queues;

    public sealed class RawUdpChannel : IChannel
    {
        public ChannelResult TryHandleInputPacket(NetworkPacket networkPacket)
        {
            return new ChannelResult(channelState: ChannelState.Accepted, networkPacket: networkPacket);
        }

        public NetworkPacket? TryHandleOutputPacket(
            NetworkPacket networkPacket)
        {
            return networkPacket;
        }

        public NetworkPacket? HandleAck(
            NetworkPacket networkPacket)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NetworkPacket> GetPendingPackets()
        {
            return Enumerable.Empty<NetworkPacket>();
        }

        public void Resend(IAsyncQueue<NetworkPacket> outputQueue)
        {
        }

        public void Flush()
        {
            throw new NotImplementedException();
        }
    }
}