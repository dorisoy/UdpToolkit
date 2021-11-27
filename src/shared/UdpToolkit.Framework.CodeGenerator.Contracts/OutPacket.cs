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
    public sealed class OutPacket : IDisposable
    {
        private readonly ConcurrentPool<OutPacket> _pool;
        private int _referencesCounter;

        /// <summary>
        /// Initializes a new instance of the <see cref="OutPacket"/> class.
        /// </summary>
        /// <param name="pool">Instance of out packets pool.</param>
        public OutPacket(ConcurrentPool<OutPacket> pool)
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
        /// <param name="event">Instance of user-defined event.</param>
        public void Setup(
            byte channelId,
            IDisposable @event)
        {
            _referencesCounter++;
            ConnectionId = default;
            IpV4Address = default;
            ChannelId = channelId;
            Event = @event;
        }

        /// <summary>
        /// Setup.
        /// </summary>
        /// <param name="channelId">Channel identifier.</param>
        /// <param name="ipV4Address">Destination ip address.</param>
        /// <param name="connectionId">Connection identifier.</param>
        /// <param name="event">Instance of user-defined event.</param>
        public void Setup(
            byte channelId,
            IpV4Address ipV4Address,
            Guid connectionId,
            IDisposable @event)
        {
            _referencesCounter++;
            ConnectionId = connectionId;
            IpV4Address = ipV4Address;
            ChannelId = channelId;
            Event = @event;
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
