namespace Structs.Contracts
{
    using System;
    using UdpToolkit.Annotations;

    [UdpEvent]
    public readonly struct Respawn
    {
        public Respawn(
            int nickname,
            Guid groupId)
        {
            Nickname = nickname;
            GroupId = groupId;
        }

        public int Nickname { get; }

        public Guid GroupId { get; }
    }
}