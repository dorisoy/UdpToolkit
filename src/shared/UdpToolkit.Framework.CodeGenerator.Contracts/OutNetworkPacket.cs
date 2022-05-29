// ReSharper disable once CheckNamespace
namespace UdpToolkit.Framework.Contracts
{
    using System;
    using System.Collections.Generic;
    using UdpToolkit.Framework.CodeGenerator.Contracts;
    using UdpToolkit.Network.Contracts.Connections;
    using UdpToolkit.Network.Contracts.Pooling;
    using UdpToolkit.Network.Contracts.Sockets;

    /// <summary>
    /// Outgoing host packet.
    /// </summary>
    public sealed class OutNetworkPacket : IDisposable
    {
        private readonly ConcurrentPool<OutNetworkPacket> _pool;
        private int _referencesCounter;

        /// <summary>
        /// Initializes a new instance of the <see cref="OutNetworkPacket"/> class.
        /// </summary>
        /// <param name="pool">Instance of out packets pool.</param>
        public OutNetworkPacket(ConcurrentPool<OutNetworkPacket> pool)
        {
            _pool = pool;
            Connections = new List<IConnection>();
            BufferWriter = new BufferWriter<byte>(2048);
        }

        /// <summary>
        /// Gets list connections for broadcast.
        /// </summary>
        public List<IConnection> Connections { get; }

        /// <summary>
        /// Gets user-defined object instance.
        /// </summary>
        public IDisposable Event { get; private set; }

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
        public BufferWriter<byte> BufferWriter { get; }

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
            _referencesCounter++;
            ConnectionId = connectionId;
            IpV4Address = ipV4Address;
            ChannelId = channelId;
            Event = @event;
            DataType = dataType;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _referencesCounter--;
            if (_referencesCounter == 0)
            {
                _referencesCounter = 0;
                ConnectionId = default;
                IpV4Address = default;
                DataType = default;
                Event?.Dispose();
                Connections.Clear();
                BufferWriter.Clear();
                ChannelId = default;
                _pool.Return(this);
            }
#if DEBUG
            else
            {
                if (_referencesCounter < 0)
                {
#pragma warning disable
                    throw new Exception("Possible bug, negative references counter!");
#pragma warning restore
                }
            }
#endif
        }
    }
}
