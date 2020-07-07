namespace SimpleUdp.Contracts
{
    using System;
    using MessagePack;

    [MessagePackObject]

    public class JoinEvent
    {
        [Obsolete(message: "For deserialization only", error: true)]
        public JoinEvent()
        {
        }

        public JoinEvent(
            byte roomId,
            string nickname)
        {
            RoomId = roomId;
            Nickname = nickname;
        }

        [Key(0)]
        public byte RoomId { get; set; }

        [Key(1)]
        public string Nickname { get; set; }
    }
}