namespace UdpToolkit.Network.Channels
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Net;
    using UdpToolkit.Network.Packets;

    [ExcludeFromCodeCoverage]
    public sealed class ReliableOrderedChannel : IChannel
    {
        public bool HandleInputPacket(NetworkPacket networkPacket)
        {
            throw new System.NotImplementedException();
        }

        public NetworkPacket GetAck(
            NetworkPacket networkPacket,
            IPEndPoint ipEndPoint)
        {
            throw new System.NotImplementedException();
        }

        public void HandleOutputPacket(NetworkPacket networkPacket)
        {
        }

        public bool HandleAck(
            NetworkPacket networkPacket)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<NetworkPacket> ToResend()
        {
            return Enumerable.Empty<NetworkPacket>();
        }
    }
}