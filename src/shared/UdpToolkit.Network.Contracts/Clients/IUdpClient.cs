namespace UdpToolkit.Network.Contracts.Clients
{
    using System;
    using System.Threading;
    using UdpToolkit.Network.Contracts.Sockets;

    /// <summary>
    /// Custom UDP client with own network protocol.
    /// </summary>
    public interface IUdpClient : IDisposable
    {
        /// <summary>
        /// Raised when new user-defined packet received.
        /// </summary>
        /// <remarks>
        /// IpV4Address - source ip
        /// Guid - connectionId
        /// byte[] - payload without network header
        /// byte - channel
        /// byte - dataType
        /// </remarks>
        event Action<IpV4Address, Guid, byte[], byte, byte> OnPacketReceived;

        /// <summary>
        /// Raised when outgoing packet dropped.
        /// </summary>
        /// <remarks>
        /// IpV4Address - source ip
        /// Guid - connectionId
        /// byte[] - payload without network header
        /// byte - channel
        /// byte - dataType
        /// </remarks>
        event Action<IpV4Address, Guid, byte[], byte, byte> OnPacketDropped;

        /// <summary>
        /// Raised when received packet with invalid network header.
        /// </summary>
        /// <remarks>
        /// IpV4Address - source ip
        /// Guid - connectionId
        /// byte[] - payload without network header
        /// byte - channel
        /// byte - dataType
        /// </remarks>
        public event Action<IpV4Address, byte[]> OnInvalidPacketReceived;

        /// <summary>
        /// Raised when user-defined packet expired.
        /// </summary>
        /// <remarks>
        /// IpV4Address - source ip
        /// Guid - connectionId
        /// byte[] - payload without network header
        /// byte - channel
        /// byte - dataType
        /// </remarks>
        event Action<IpV4Address, Guid, byte[], byte, byte> OnPacketExpired;

        /// <summary>
        /// Raised when UDP client connected to other UDP client.
        /// </summary>
        /// <remarks>
        /// IpV4Address - source ip
        /// Guid - connectionId
        /// </remarks>
        event Action<IpV4Address, Guid> OnConnected;

        /// <summary>
        /// Raised when UDP client disconnected from other UDP client.
        /// </summary>
        /// <remarks>
        /// IpV4Address - source ip
        /// Guid - connectionId
        /// </remarks>
        event Action<IpV4Address, Guid> OnDisconnected;

        /// <summary>
        /// Raised when heartbeat received.
        /// </summary>
        /// <remarks>
        /// Guid - connectionId
        /// TimeSpan - rtt for specific connection
        /// </remarks>
        event Action<Guid, TimeSpan> OnHeartbeat;

        /// <summary>
        /// Check state of connection to another UDP client.
        /// </summary>
        /// <param name="connectionId">Own connectionId.</param>
        /// <returns>true if connection alive, false - if connection expired or client not connected.</returns>
        bool IsConnected(
            out Guid connectionId);

        /// <summary>
        /// Connect to another UDP client.
        /// </summary>
        /// <param name="ipV4Address">Destination ip address.</param>
        /// <param name="connectionId">Connection identifier.</param>
        void Connect(
            IpV4Address ipV4Address,
            Guid? connectionId);

        /// <summary>
        /// Send heartbeat.
        /// </summary>
        /// <param name="ipV4Address">Destination ip address.</param>
        void Heartbeat(
            IpV4Address ipV4Address);

        /// <summary>
        /// Disconnect from another UDP client.
        /// </summary>
        /// <param name="ipV4Address">Another UDP client address.</param>
        void Disconnect(
            IpV4Address ipV4Address);

        /// <summary>
        /// Send user-defined packets.
        /// </summary>
        /// <param name="connectionId">Connection identifier.</param>
        /// <param name="channelId">Channel identifier.</param>
        /// <param name="dataType">Type of data.</param>
        /// <param name="bytes">Payload.</param>
        /// <param name="ipV4Address">Destination ip address.</param>
        void Send(
            Guid connectionId,
            byte channelId,
            byte dataType,
            byte[] bytes,
            IpV4Address ipV4Address);

        /// <summary>
        /// Start receive packets.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token for cancelling receive operation.</param>
        void StartReceive(
            CancellationToken cancellationToken);

        /// <summary>
        /// Get local ip address of udp client.
        /// </summary>
        /// <returns>Local ip address of udp client.</returns>
        IpV4Address GetLocalIp();
    }
}