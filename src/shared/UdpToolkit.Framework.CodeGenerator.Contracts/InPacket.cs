// ReSharper disable once CheckNamespace
namespace UdpToolkit.Framework.Contracts
{
    using System;
    using UdpToolkit.Network.Contracts.Sockets;

    /// <summary>
    /// Incoming host packet.
    /// </summary>
    public readonly struct InPacket
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InPacket"/> struct.
        /// </summary>
        /// <param name="payload">Bytes array of incoming data.</param>
        /// <param name="connectionId">Connection identifier.</param>
        /// <param name="ipV4Address">Source ip address.</param>
        /// <param name="channelId">Channel identifier.</param>
        /// <param name="expired">Value indicating whether the expiration state.</param>
        public InPacket(
            byte[] payload,
            Guid connectionId,
            IpV4Address ipV4Address,
            byte channelId,
            bool expired)
        {
            Payload = payload;
            ConnectionId = connectionId;
            IpV4Address = ipV4Address;
            ChannelId = channelId;
            Expired = expired;
        }

        /// <summary>
        /// Gets channel identifier.
        /// </summary>
        public byte ChannelId { get; }

        /// <summary>
        /// Gets bytes array of incoming data.
        /// </summary>
        public byte[] Payload { get; }

        /// <summary>
        /// Gets connection identifier.
        /// </summary>
        public Guid ConnectionId { get; }

        /// <summary>
        /// Gets source ip address.
        /// </summary>
        public IpV4Address IpV4Address { get; }

        /// <summary>
        /// Gets a value indicating whether the expiration state.
        /// </summary>
        public bool Expired { get; }
    }
}