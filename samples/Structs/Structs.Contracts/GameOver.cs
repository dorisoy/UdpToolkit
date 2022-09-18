namespace Structs.Contracts
{
    using System;
    using UdpToolkit.Annotations;

    public enum Reason
    {
        GameOver,
    }

    [UdpEvent]
    public readonly struct GameOver
    {
        public GameOver(
            Reason message,
            Guid groupId)
        {
            Message = message;
            GroupId = groupId;
        }

        public Reason Message { get; }

        public Guid GroupId { get; }
    }
}