namespace ReliableUdp.Contracts
{
    using System;
    using UdpToolkit.Annotations;

    [UdpEvent]
    public class Respawn
    {
        public Respawn(
            string nickname,
            Guid roomId)
        {
            Nickname = nickname;
            RoomId = roomId;
        }

        public Guid RoomId { get; }

        public string Nickname { get; }
    }
}