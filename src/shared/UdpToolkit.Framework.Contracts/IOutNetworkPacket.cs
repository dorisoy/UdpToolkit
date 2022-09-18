namespace UdpToolkit.Framework.Contracts
{
    using System;
    using UdpToolkit.Framework.CodeGenerator.Contracts;
    using UdpToolkit.Network.Contracts.Sockets;
    using UdpToolkit.Serialization;

    /// <summary>
    /// Abstraction for representing out network packet.
    /// </summary>
    public interface IOutNetworkPacket : IDisposable
    {
        /// <summary>
        /// Gets channel identifier.
        /// </summary>
        byte ChannelId { get; }

        /// <summary>
        /// Gets type of data.
        /// </summary>
        byte DataType { get; }

        /// <summary>
        /// Gets connection identifier.
        /// </summary>
        Guid ConnectionId { get; }

        /// <summary>
        /// Gets source ip address.
        /// </summary>
        IpV4Address IpV4Address { get; }

        /// <summary>
        /// Gets buffer writer.
        /// </summary>
        BufferWriter<byte> BufferWriter { get; }

        /// <summary>
        /// Serialization.
        /// </summary>
        /// <param name="serializer">Serialization strategy.</param>
        void Serialize(ISerializer serializer);
    }
}