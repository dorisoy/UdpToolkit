namespace Shared.Spawn
{
    using MessagePack;

    [MessagePackObject]
    public class SpawnEvent
    {
        [Key(0)]
        public byte RoomId { get; set; }

        [Key(1)]
        public string Nickname { get; set; }
    }
}