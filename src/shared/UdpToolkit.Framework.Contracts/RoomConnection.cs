namespace UdpToolkit.Framework.Contracts
{
    using System;
    using UdpToolkit.Network.Contracts.Sockets;

    public readonly struct RoomConnection
    {
        public RoomConnection(
            Guid connectionId,
            IpV4Address ipV4Address)
        {
            ConnectionId = connectionId;
            IpV4Address = ipV4Address;
        }

        public Guid ConnectionId { get; }

        public IpV4Address IpV4Address { get; }
    }
}