namespace UdpToolkit.Network.Contracts.Clients
{
    using System;
    using System.Buffers;
    using UdpToolkit.Network.Contracts.Pooling;
    using UdpToolkit.Network.Contracts.Sockets;

    /// <summary>
    /// Represent a network packet.
    /// </summary>
    public sealed class InNetworkPacket : IDisposable
    {
        private readonly ArrayPool<byte> _arrayPool;
        private readonly ConcurrentPool<InNetworkPacket> _networkPacketsPool;

        /// <summary>
        /// Initializes a new instance of the <see cref="InNetworkPacket"/> class.
        /// </summary>
        /// <param name="networkPacketsPool">Instance of network packets pool.</param>
        /// <param name="arrayPool">Array pool.</param>
        public InNetworkPacket(
            ArrayPool<byte> arrayPool,
            ConcurrentPool<InNetworkPacket> networkPacketsPool)
        {
            _arrayPool = arrayPool;
            _networkPacketsPool = networkPacketsPool;
        }

        /// <summary>
        /// Gets source ip address.
        /// </summary>
        public IpV4Address IpV4Address { get; private set; }

        /// <summary>
        /// Gets connection identifier.
        /// </summary>
        public Guid ConnectionId { get; private set; }

        /// <summary>
        /// Gets routing key.
        /// </summary>
        public Guid RoutingKey { get; private set; }

        /// <summary>
        /// Gets pooled bytes buffer.
        /// </summary>
        public byte[] Buffer { get; private set; }

        /// <summary>
        /// Gets channel id.
        /// </summary>
        public byte ChannelId { get; private set; }

        /// <summary>
        /// Gets received bytes count.
        /// </summary>
        public int BytesReceived { get; private set; }

        /// <summary>
        /// Gets type of data.
        /// </summary>
        public byte DataType { get; private set; }

        /// <summary>
        /// Gets a value indicating whether packet is expired.
        /// </summary>
        public bool Expired { get; private set; }

        /// <summary>
        /// Setup values for object.
        /// </summary>
        /// <param name="buffer">Pooled buffer with received data.</param>
        /// <param name="ipV4">Source ip address.</param>
        /// <param name="connectionId">Connection identifier.</param>
        /// <param name="routingKey">Routing key.</param>
        /// <param name="channelId">Channel identifier.</param>
        /// <param name="dataType">Type of data.</param>
        /// <param name="bytesReceived">Received bytes count.</param>
        /// <param name="isExpired">IsExpired.</param>
        public void Setup(
            byte[] buffer,
            IpV4Address ipV4,
            Guid connectionId,
            Guid routingKey,
            byte channelId,
            byte dataType,
            int bytesReceived,
            bool isExpired)
        {
            Buffer = buffer;
            IpV4Address = ipV4;
            ConnectionId = connectionId;
            ChannelId = channelId;
            DataType = dataType;
            BytesReceived = bytesReceived;
            Expired = isExpired;
            RoutingKey = routingKey;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _arrayPool.Return(Buffer, true);

            IpV4Address = default;
            ConnectionId = default;
            RoutingKey = default;
            ChannelId = default;
            DataType = default;
            BytesReceived = default;
            _networkPacketsPool.Return(this);
        }
    }
}