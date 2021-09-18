namespace ReliableUdp.Contracts
{
    using System;
    using UdpToolkit.Annotations;

    [UdpEvent]
    public class JoinEvent
    {
        public JoinEvent(
            Guid groupId,
            string nickname)
        {
            GroupId = groupId;
            Nickname = nickname;
        }

        public Guid GroupId { get; }

        public string Nickname { get; }
    }
}