namespace ReliableUdp.Contracts
{
    using System;
    using System.Collections.Generic;
    using MessagePack;
    using UdpToolkit.Annotations;
    using UnityEngine;

    [UdpEvent]
    [MessagePackObject]
    public class StartGame
    {
        public StartGame(
            int roomId,
            Dictionary<Guid, Vector3> positions)
        {
            RoomId = roomId;
            Positions = positions;
        }

        [Key(0)]
        public int RoomId { get; }

        [Key(1)]
        public Dictionary<Guid, Vector3> Positions { get; }
    }
}