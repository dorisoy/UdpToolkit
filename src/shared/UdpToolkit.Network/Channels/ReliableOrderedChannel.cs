namespace UdpToolkit.Network.Channels
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using UdpToolkit.Network.Packets;

    [ExcludeFromCodeCoverage]
    public sealed class ReliableOrderedChannel : IChannel
    {
        public bool HandleInputPacket(NetworkPacket networkPacket)
        {
            throw new System.NotImplementedException();
        }

        public NetworkPacket GetAck(
            NetworkPacket networkPacket)
        {
            throw new System.NotImplementedException();
        }

        public void HandleOutputPacket(NetworkPacket networkPacket)
        {
        }

        public void GetNext(NetworkPacket networkPacket)
        {
        }

        public bool HandleAck(
            NetworkPacket networkPacket)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<NetworkPacket> ToResend(TimeSpan resendTimeout)
        {
            return Enumerable.Empty<NetworkPacket>();
        }
    }
}