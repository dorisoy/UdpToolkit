namespace Cubes.Shared.Events
{
    using MessagePack;

    [MessagePackObject]
    public class JoinEvent
    {
        public JoinEvent(
            int roomId,
            string nickname)
        {
            RoomId = roomId;
            Nickname = nickname;
        }

        [Key(0)]
        public int RoomId { get; }

        [Key(1)]
        public string Nickname { get; }
    }
}