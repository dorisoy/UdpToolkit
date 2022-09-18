namespace Structs.Contracts
{
    using System;
    using UdpToolkit.Annotations;

    [UdpEvent]
    public readonly struct Death
    {
        public Death(
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