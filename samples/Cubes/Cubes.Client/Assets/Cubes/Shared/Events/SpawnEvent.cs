namespace Cubes.Shared.Events
{
    using System;
    using MessagePack;
    using UnityEngine;

    [MessagePackObject]
    public class SpawnEvent
    {
        public SpawnEvent(
            Guid playerId,
            int roomId,
            string nickname,
            Vector3 position)
        {
            PlayerId = playerId;
            RoomId = roomId;
            Nickname = nickname;
            Position = position;
        }

        [Key(0)]
        public Guid PlayerId { get; }

        [Key(1)]
        public int RoomId { get; }

        [Key(2)]
        public string Nickname { get; }

        [Key(3)]
        public Vector3 Position { get; }
    }
}