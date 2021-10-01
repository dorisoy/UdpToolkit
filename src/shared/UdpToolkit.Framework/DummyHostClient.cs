namespace UdpToolkit.Framework
{
    using System;
    using UdpToolkit.Framework.Contracts;
    using UdpToolkit.Network.Contracts.Sockets;

    /// <summary>
    /// Null object.
    /// </summary>
    public sealed class DummyHostClient : IHostClient
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DummyHostClient"/> class.
        /// </summary>
        public DummyHostClient()
        {
            OnDisconnected += (ipV4, connectionId) => { };
            OnConnected += (ipV4, connectionId) => { };
            OnRttReceived += rtt => { };
            OnConnectionTimeout += () => { };

            OnConnectionTimeout?.Invoke();
            OnConnected?.Invoke(default, default);
            OnDisconnected?.Invoke(default, default);
            OnRttReceived?.Invoke(default);
        }

        /// <inheritdoc />
        public event Action<IpV4Address, Guid> OnDisconnected;

        /// <inheritdoc />
        public event Action<IpV4Address, Guid> OnConnected;

        /// <inheritdoc />
        public event Action OnConnectionTimeout;

        /// <inheritdoc />
        public event Action<double> OnRttReceived;

        /// <inheritdoc />
        public void Connect(
            Guid? connectionId = null)
        {
            // nothing to do
        }

        /// <inheritdoc />
        public void Connect(
            string host,
            int port,
            Guid? connectionId = null)
        {
            // nothing to do
        }

        /// <inheritdoc />
        public void Disconnect(
            string host,
            int port)
        {
            // nothing to do
        }

        /// <inheritdoc />
        public void Disconnect()
        {
            // nothing to do
        }

        /// <inheritdoc />
        public void Send<TEvent>(
            TEvent @event,
            byte channelId)
        {
            // nothing to do
        }

        /// <inheritdoc />
        public void Send<TEvent>(
            TEvent @event,
            IpV4Address destination,
            byte channelId)
        {
            // nothing to do
        }

        /// <inheritdoc />
        public void Dispose()
        {
            // nothing to do
        }
    }
}