namespace UdpToolkit.Benchmarks.Hubs.CurrentSolution
{
    using System;
    using MessagePack;
    using UnityEngine;

    [MessagePackObject]
    public class BenchmarkEvent
    {
        [Obsolete(message: "For deserialization only", error: true)]
        public BenchmarkEvent()
        {
        }

        public BenchmarkEvent(
            byte playerId,
            byte roomId,
            float distance,
            float angle,
            Quaternion rotation,
            Vector3 direction,
            Vector3 position)
        {
            PlayerId = playerId;
            RoomId = roomId;
            Distance = distance;
            Angle = angle;
            Rotation = rotation;
            Direction = direction;
            Position = position;
        }

        [Key(0)]
        public byte PlayerId { get; set; }

        [Key(1)]
        public byte RoomId { get; set; }

        [Key(2)]
        public float Distance { get; set; }

        [Key(3)]
        public float Angle { get; set; }

        [Key(4)]
        public Quaternion Rotation { get; set; }

        [Key(5)]
        public Vector3 Direction { get; set; }

        [Key(6)]
        public Vector3 Position { get; set; }
    }
}