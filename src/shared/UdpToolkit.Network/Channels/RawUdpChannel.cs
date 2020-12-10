namespace UdpToolkit.Network.Channels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using UdpToolkit.Network.Packets;

    public sealed class RawUdpChannel : IChannel
    {
        public bool HandleInputPacket(NetworkPacket networkPacket)
        {
            return true;
        }

        public NetworkPacket GetAck(
            NetworkPacket networkPacket)
        {
            throw new NotImplementedException();
        }

        public void HandleOutputPacket(
            NetworkPacket networkPacket)
        {
        }

        public void GetNext(NetworkPacket networkPacket)
        {
        }

        public bool HandleAck(
            NetworkPacket networkPacket)
        {
            return true;
        }

        public IEnumerable<NetworkPacket> ToResend(TimeSpan resendTimeout)
        {
            return Enumerable.Empty<NetworkPacket>();
        }
    }
}