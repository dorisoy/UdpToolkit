namespace ReliableUdp.Contracts
{
    using System;
    using UdpToolkit.Annotations;

    [UdpEvent]
    public class GameOver
    {
        public GameOver(
            Guid roomId,
            string message)
        {
            RoomId = roomId;
            Message = message;
        }

        public string Message { get; }

        public Guid RoomId { get; }
    }
}