namespace ReliableUdp.Contracts
{
    using System;
    using UdpToolkit.Annotations;

    [UdpEvent]
    public class GameOver
    {
        public GameOver(
            Guid groupId,
            string message)
        {
            GroupId = groupId;
            Message = message;
        }

        public string Message { get; }

        public Guid GroupId { get; }
    }
}