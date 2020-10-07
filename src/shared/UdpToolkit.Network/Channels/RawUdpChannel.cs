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
            NetworkPacket networkPacket,
            IPEndPoint ipEndPoint)
        {
            throw new NotImplementedException();
        }

        public void HandleOutputPacket(
            NetworkPacket networkPacket)
        {
        }

        public bool HandleAck(
            NetworkPacket networkPacket)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NetworkPacket> ToResend()
        {
            return Enumerable.Empty<NetworkPacket>();
        }
    }
}