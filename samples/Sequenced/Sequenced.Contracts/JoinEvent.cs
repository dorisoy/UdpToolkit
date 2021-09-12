namespace Sequenced.Contracts
{
    using System;
    using MessagePack;
    using UdpToolkit.Annotations;

    [UdpEvent]
    [MessagePackObject]
    public class JoinEvent
    {
        public JoinEvent(
            Guid roomId,
            string nickname)
        {
            RoomId = roomId;
            Nickname = nickname;
        }

        [Key(0)]
        public Guid RoomId { get; set; }

        [Key(1)]
        public string Nickname { get; set; }
    }
}