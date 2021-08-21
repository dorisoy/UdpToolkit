// ReSharper disable once CheckNamespace
namespace UdpToolkit.Framework.Contracts
{
    using System;
    using System.Collections.Generic;

    public class Room
    {
        public Room(
            int id,
            List<RoomConnection> roomConnections,
            DateTimeOffset createdAt)
        {
            RoomConnections = roomConnections;
            CreatedAt = createdAt;
            Id = id;
        }

        public int Id { get; }

        public List<RoomConnection> RoomConnections { get; }

        public DateTimeOffset CreatedAt { get; }
    }
}