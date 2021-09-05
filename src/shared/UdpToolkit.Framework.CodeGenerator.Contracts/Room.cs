// ReSharper disable once CheckNamespace
namespace UdpToolkit.Framework.Contracts
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Room.
    /// </summary>
    public class Room
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Room"/> class.
        /// </summary>
        /// <param name="id">Room identifier.</param>
        /// <param name="roomConnections">List for room connections.</param>
        /// <param name="createdAt">Date of creation.</param>
        public Room(
            int id,
            List<RoomConnection> roomConnections,
            DateTimeOffset createdAt)
        {
            RoomConnections = roomConnections;
            CreatedAt = createdAt;
            Id = id;
        }

        /// <summary>
        /// Gets room identifier.
        /// </summary>
        public int Id { get; }

        /// <summary>
        /// Gets list of room connections.
        /// </summary>
        public List<RoomConnection> RoomConnections { get; }

        /// <summary>
        /// Gets creation date.
        /// </summary>
        public DateTimeOffset CreatedAt { get; }
    }
}