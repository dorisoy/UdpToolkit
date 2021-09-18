namespace P2P.Contracts
{
    using System;
    using MessagePack;
    using UdpToolkit.Annotations;

    [UdpEvent]
    [MessagePackObject]
    public class JoinEvent
    {
        public JoinEvent(
            Guid groupId,
            string nickname)
        {
            GroupId = groupId;
            Nickname = nickname;
        }

        [Key(0)]
        public Guid GroupId { get; set; }

        [Key(1)]
        public string Nickname { get; set; }
    }
}