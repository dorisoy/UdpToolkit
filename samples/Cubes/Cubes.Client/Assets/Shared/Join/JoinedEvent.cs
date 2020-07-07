namespace Shared.Join
{
    using MessagePack;

    [MessagePackObject]
    public class JoinedEvent
    {
        [Key(0)]
        public string Nickname { get; set; }
    }
}