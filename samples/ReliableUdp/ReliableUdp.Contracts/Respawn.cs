namespace ReliableUdp.Contracts
{
    using System;
    using UdpToolkit.Annotations;

    [UdpEvent]
    public class Respawn
    {
        public Respawn(
            string nickname,
            Guid groupId)
        {
            Nickname = nickname;
            GroupId = groupId;
        }

        public Guid GroupId { get; }

        public string Nickname { get; }
    }
}