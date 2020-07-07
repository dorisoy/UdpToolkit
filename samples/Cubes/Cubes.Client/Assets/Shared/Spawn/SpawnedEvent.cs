namespace Shared.Spawn
{
    using MessagePack;
    using UnityEngine;

    [MessagePackObject]
    public class SpawnedEvent
    {
        [Key(0)]
        public byte PlayerId { get; set; }

        [Key(1)]
        public string Nickname { get; set; }

        [Key(2)]
        public Vector3 Position { get; set; }
    }
}