namespace UdpToolkit.Network.Contracts.Connections
{
    using System;
    using System.Collections.Generic;
    using UdpToolkit.Network.Contracts.Channels;
    using UdpToolkit.Network.Contracts.Packets;
    using UdpToolkit.Network.Contracts.Sockets;

    /// <summary>
    /// Abstraction for representing a connection.
    /// </summary>
    public interface IConnection
    {
        /// <summary>
        /// Gets list of pending packets.
        /// </summary>
        /// <returns>List of pending packets.</returns>
        IList<PendingPacket> PendingPackets { get; }

        /// <summary>
        /// Gets connection identifier.
        /// </summary>
        Guid ConnectionId { get; }

        /// <summary>
        /// Gets a value indicating whether needs to remove the connection from the pool.
        /// </summary>
        /// <remarks>
        /// true - housekeeper would ignore this connection on the cleanup scan.
        /// false - housekeeper may remove this connection if the connection expired.
        /// </remarks>
        bool KeepAlive { get; }

        /// <summary>
        /// Gets a value of connection ip address.
        /// </summary>
        IpV4Address IpV4Address { get; }

        /// <summary>
        /// Gets a date time of the last heartbeat.
        /// </summary>
        DateTimeOffset LastHeartbeat { get; }

        /// <summary>
        /// Get channel for incoming packet by channel id.
        /// </summary>
        /// <param name="channelId">Channel identifier.</param>
        /// <param name="channel">Channel.</param>
        /// <returns>True if channel exists.</returns>
        bool GetIncomingChannel(
            byte channelId,
            out IChannel channel);

        /// <summary>
        /// Get channel for outgoing packet by channel id.
        /// </summary>
        /// <param name="channelId">Channel identifier.</param>
        /// <param name="channel">Channel.</param>
        /// <returns>True if channel exists.</returns>
        bool GetOutgoingChannel(
            byte channelId,
            out IChannel channel);

        /// <summary>
        /// Updates HeartbeatAck value.
        /// </summary>
        /// <param name="utcNow">DateTime utc now.</param>
        void OnHeartbeatAck(
            DateTimeOffset utcNow);

        /// <summary>
        /// Updates LastHeartbeat value.
        /// </summary>
        /// <param name="utcNow">DateTime utc now.</param>
        void OnHeartbeat(
            DateTimeOffset utcNow);

        /// <summary>
        /// Gets current RTT for connection.
        /// </summary>
        /// <returns>RTT in ms.</returns>
        TimeSpan GetRtt();
    }
}