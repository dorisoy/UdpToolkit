namespace ReliableUdp.Contracts
{
    using System;
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
        public int RoomId { get; set; }

        [Key(1)]
        public string Nickname { get; set; }
    }
}