// ReSharper disable once CheckNamespace
namespace UdpToolkit.Framework.Contracts
{
    using System;
    using UdpToolkit.Network.Contracts.Sockets;

    /// <summary>
    /// Outgoing host packet.
    /// </summary>
    public readonly struct OutPacket
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OutPacket"/> struct.
        /// </summary>
        /// <param name="connectionId">Connection identifier.</param>
        /// <param name="channelId">Channel identifier.</param>
        /// <param name="event">Instance of user-defined event.</param>
        /// <param name="ipV4Address">Destination ip address.</param>
        public OutPacket(
            Guid connectionId,
            byte channelId,
            object @event,
            IpV4Address ipV4Address)
        {
            ChannelId = channelId;
            Event = @event;
            IpV4Address = ipV4Address;
            ConnectionId = connectionId;
        }

        /// <summary>
        /// Gets connection identifier.
        /// </summary>
        public Guid ConnectionId { get; }

        /// <summary>
        /// Gets event instance.
        /// </summary>
        public object Event { get; }

        /// <summary>
        /// Gets channel identifier.
        /// </summary>
        public byte ChannelId { get; }

        /// <summary>
        /// Gets destination ip address.
        /// </summary>
        public IpV4Address IpV4Address { get; }
    }
}
