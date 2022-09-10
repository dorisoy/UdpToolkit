// ReSharper disable once CheckNamespace
namespace UdpToolkit.Framework.Contracts
{
    using System;
    using UdpToolkit.Framework.CodeGenerator.Contracts;
    using UdpToolkit.Network.Contracts.Pooling;
    using UdpToolkit.Network.Contracts.Sockets;

    /// <summary>
    /// Outgoing host packet.
    /// </summary>
    public sealed class OutNetworkPacket : IDisposable
    {
        private readonly ConcurrentPool<OutNetworkPacket> _pool;

        /// <summary>
        /// Initializes a new instance of the <see cref="OutNetworkPacket"/> class.
        /// </summary>
        /// <param name="pool">Instance of out packets pool.</param>
        public OutNetworkPacket(ConcurrentPool<OutNetworkPacket> pool)
        {
            _pool = pool;
        }

        /// <summary>
        /// Gets user-defined event.
        /// </summary>
        /// <remarks>
        /// object instead IDisposable for avoid serialization issue with MessagePack.
        /// </remarks>
        public object Event { get; private set; }

        /// <summary>
        /// Gets channel identifier.
        /// </summary>
        public byte ChannelId { get; private set; }

        /// <summary>
        /// Gets type of data.
        /// </summary>
        public byte DataType { get; private set; }

        /// <summary>
        /// Gets connection identifier.
        /// </summary>
        public Guid ConnectionId { get; private set; }

        /// <summary>
        /// Gets source ip address.
        /// </summary>
        public IpV4Address IpV4Address { get; private set; }

        /// <summary>
        /// Gets buffer writer.
        /// </summary>
        public BufferWriter<byte> BufferWriter { get; private set; }

        /// <summary>
        /// Setup.
        /// </summary>
        /// <param name="channelId">Channel identifier.</param>
        /// <param name="ipV4Address">Destination ip address.</param>
        /// <param name="connectionId">Connection identifier.</param>
        /// <param name="dataType">Type of data.</param>
        /// <param name="event">Instance of user-defined event.</param>
        public void Setup(
            byte channelId,
            IpV4Address ipV4Address,
            Guid connectionId,
            byte dataType,
            IDisposable @event)
        {
            ConnectionId = connectionId;
            IpV4Address = ipV4Address;
            ChannelId = channelId;
            DataType = dataType;
            Event = @event;
        }

        /// <summary>
        /// Setup.
        /// </summary>
        /// <param name="channelId">Channel identifier.</param>
        /// <param name="ipV4Address">Destination ip address.</param>
        /// <param name="connectionId">Connection identifier.</param>
        /// <param name="dataType">Type of data.</param>
        /// <param name="bufferWriter">Instance of buffer writer.</param>
        public void Setup(
            byte channelId,
            IpV4Address ipV4Address,
            Guid connectionId,
            byte dataType,
            BufferWriter<byte> bufferWriter)
        {
            ConnectionId = connectionId;
            IpV4Address = ipV4Address;
            ChannelId = channelId;
            DataType = dataType;
            BufferWriter = bufferWriter;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            ConnectionId = default;
            IpV4Address = default;
            DataType = default;
            BufferWriter?.Dispose();

            // HACK for avoid serialization issue with MessagePack
            if (Event is IDisposable disposable)
            {
                disposable.Dispose();
            }

            Event = default;
            BufferWriter = default;
            ChannelId = default;
            _pool.Return(this);
        }
    }
}
