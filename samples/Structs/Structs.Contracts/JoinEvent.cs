namespace Structs.Contracts
{
    using System;
    using UdpToolkit.Annotations;

    [UdpEvent]
    public readonly struct JoinEvent
    {
        public JoinEvent(
            Guid groupId,
            int nickname)
        {
            GroupId = groupId;
            Nickname = nickname;
        }

        public Guid GroupId { get; }

        public int Nickname { get; }
    }
}