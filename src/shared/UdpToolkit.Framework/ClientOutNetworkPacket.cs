namespace UdpToolkit.Framework
{
    using System;
    using UdpToolkit.Framework.CodeGenerator.Contracts;
    using UdpToolkit.Framework.Contracts;
    using UdpToolkit.Network.Contracts.Sockets;
    using UdpToolkit.Serialization;

    /// <inheritdoc cref="UdpToolkit.Framework.Contracts.IOutNetworkPacket" />
    public sealed class ClientOutNetworkPacket<T> : IOutNetworkPacket, IClientOutNetworkPacket
        where T : class, IDisposable
    {
        /// <summary>
        /// Gets instance of event.
        /// </summary>
        public T Event { get; private set; }

        /// <inheritdoc />
        public byte ChannelId { get; private set; }

        /// <inheritdoc />
        public byte DataType { get; private set; }

        /// <inheritdoc />
        public Guid ConnectionId { get; private set; }

        /// <inheritdoc />
        public IpV4Address IpV4Address { get; private set; }

        /// <inheritdoc />
        public BufferWriter<byte> BufferWriter { get; private set; }

        /// <summary>
        /// Setup.
        /// </summary>
        /// <param name="channelId">Channel identifier.</param>
        /// <param name="ipV4Address">Destination ip address.</param>
        /// <param name="connectionId">Connection identifier.</param>
        /// <param name="dataType">Type of data.</param>
        /// <param name="event">Instance of user-defined event.</param>
        /// <param name="bufferWriter">Instance of buffer writer.</param>
        public void Setup(
            byte channelId,
            IpV4Address ipV4Address,
            Guid connectionId,
            byte dataType,
            T @event,
            BufferWriter<byte> bufferWriter)
        {
            ChannelId = channelId;
            IpV4Address = ipV4Address;
            ConnectionId = connectionId;
            DataType = dataType;
            Event = @event;
            BufferWriter = bufferWriter;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            ChannelId = default;
            DataType = default;
            ConnectionId = default;
            IpV4Address = default;
            BufferWriter.Dispose();
            Event.Dispose();
            ObjectsPool<ClientOutNetworkPacket<T>>.Return(this);
        }

        /// <inheritdoc />
        public void Serialize(ISerializer serializer)
        {
            serializer.Serialize(BufferWriter, Event);
        }
    }
}