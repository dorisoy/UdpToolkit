namespace ReliableUdp.Contracts
{
    using System;
    using System.Collections.Generic;
    using System.Numerics;
    using UdpToolkit.Annotations;

    [UdpEvent]
    public class StartGame
    {
        public StartGame(
            int roomId,
            Dictionary<Guid, Vector3> positions)
        {
            RoomId = roomId;
            Positions = positions;
        }

        public int RoomId { get; }

        public Dictionary<Guid, Vector3> Positions { get; }
    }
}