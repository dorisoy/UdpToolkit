namespace UdpToolkit.Network.Channels
{
    using System;
    using UdpToolkit.Network.Contracts.Channels;
    using UdpToolkit.Network.Contracts.Protocol;

    /// <summary>
    /// Raw UDP channel.
    /// </summary>
    public sealed class RawUdpChannel : IChannel
    {
        /// <summary>
        /// Reserved chanel identifier.
        /// </summary>
        public static readonly byte Id = ReliableChannelConsts.RawChannel;

        /// <inheritdoc />
        public bool ResendOnHeartbeat { get; } = false;

        /// <inheritdoc />
        public byte ChannelId { get; } = Id;

        /// <inheritdoc />
        public bool HandleInputPacket(
            in NetworkHeader networkHeader)
        {
            return true;
        }

        /// <inheritdoc />
        public bool IsDelivered(
            in NetworkHeader networkHeader)
        {
            return true;
        }

        /// <inheritdoc />
        public NetworkHeader HandleOutputPacket(
            byte dataType,
            Guid connectionId,
            PacketType packetType)
        {
            return new NetworkHeader(
                channelId: Id,
                id: default,
                acks: default,
                connectionId: connectionId,
                packetType: packetType,
                dataType: dataType);
        }

        /// <inheritdoc />
        public bool HandleAck(
            in NetworkHeader networkHeader)
        {
            return true;
        }
    }
}