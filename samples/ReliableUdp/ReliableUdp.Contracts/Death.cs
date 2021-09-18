namespace ReliableUdp.Contracts
{
    using System;
    using UdpToolkit.Annotations;

    [UdpEvent]
    public class Death
    {
        public Death(
            string nickname,
            Guid groupId)
        {
            Nickname = nickname;
            GroupId = groupId;
        }

        public string Nickname { get; }

        public Guid GroupId { get; }
    }
}