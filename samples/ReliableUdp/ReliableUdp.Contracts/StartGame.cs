namespace ReliableUdp.Contracts
{
    using System;
    using System.Collections.Generic;
    using UdpToolkit.Annotations;

    [UdpEvent]
    public class StartGame
    {
        public StartGame(
            Guid roomId,
            Dictionary<Guid, Position> positions)
        {
            RoomId = roomId;
            Positions = positions;
        }

        public Guid RoomId { get; }

        public Dictionary<Guid, Position> Positions { get; }
    }
}