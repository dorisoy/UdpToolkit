namespace Shared.Join
{
    using MessagePack;

    [MessagePackObject]
    public class JoinEvent
    {
        [Key(0)]
        public byte RoomId { get; set; }

        [Key(1)]
        public string Nickname { get; set; }
    }
}