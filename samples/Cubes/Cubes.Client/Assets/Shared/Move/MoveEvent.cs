namespace Shared.Move
{
    using MessagePack;
    using UnityEngine;

    [MessagePackObject]
    public class MoveEvent
    {
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