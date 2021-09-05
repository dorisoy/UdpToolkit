namespace UdpToolkit.Framework.Contracts
{
    using System;
    using UdpToolkit.Network.Contracts.Sockets;

    /// <summary>
    /// Client for interacting wih other hosts.
    /// </summary>
    public interface IHostClient : IDisposable
    {
        /// <summary>
        /// On disconnected from the host.
        /// </summary>
        event Action<IpV4Address, Guid> OnDisconnected;

        /// <summary>
        /// On connected to the host.
        /// </summary>
        event Action<IpV4Address, Guid> OnConnected;

        /// <summary>
        /// On connection timeout to the host.
        /// </summary>
        event Action OnConnectionTimeout;

        /// <summary>
        /// On new round trip time (ms) received.
        /// </summary>
        event Action<double> OnRttReceived;

        /// <summary>
        /// Connect to the remote host.
        /// </summary>
        void Connect();

        /// <summary>
        /// Connect to the remote host.
        /// </summary>
        /// <param name="host">The ip address of the remote host.</param>
        /// <param name="port">The port of the remote host.</param>
        void Connect(
            string host,
            int port);

        /// <summary>
        /// Disconnect from the remote host.
        /// </summary>
        void Disconnect();

        /// <summary>
        /// Disconnect from the remote host.
        /// </summary>
        /// <param name="host">The ip address of the remote host.</param>
        /// <param name="port">The port of the remote host.</param>
        void Disconnect(
            string host,
            int port);

        /// <summary>
        /// Send event to the remote host.
        /// </summary>
        /// <param name="event">user-defined event instance.</param>
        /// <param name="channelId">Channel id for sending data.</param>
        /// <typeparam name="TEvent">
        /// Type of user-defined event.
        /// </typeparam>
        void Send<TEvent>(
            TEvent @event,
            byte channelId);

        /// <summary>
        /// Send event to the remote host.
        /// </summary>
        /// <param name="event">user-defined event instance.</param>
        /// <param name="destination">Destination ip address.</param>
        /// <param name="channelId">Channel id for sending data.</param>
        /// <typeparam name="TEvent">
        /// Type of user-defined event.
        /// </typeparam>
        public void Send<TEvent>(
            TEvent @event,
            IpV4Address destination,
            byte channelId);
    }
}