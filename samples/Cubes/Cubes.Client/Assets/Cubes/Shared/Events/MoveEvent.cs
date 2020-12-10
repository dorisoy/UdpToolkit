namespace Cubes.Shared.Events
{
    using MessagePack;
    using UnityEngine;

    [MessagePackObject]
    public class MoveEvent
    {
        public MoveEvent(
            int playerId,
            int roomId,
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
        public int PlayerId { get; }

        [Key(1)]
        public int RoomId { get; }

        [Key(2)]
        public float Distance { get; }

        [Key(3)]
        public float Angle { get; }

        [Key(4)]
        public Quaternion Rotation { get; }

        [Key(5)]
        public Vector3 Direction { get; }

        [Key(6)]
        public Vector3 Position { get; }
    }
}