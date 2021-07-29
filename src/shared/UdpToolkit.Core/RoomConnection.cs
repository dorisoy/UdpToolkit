namespace UdpToolkit.Core
{
    using System;

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