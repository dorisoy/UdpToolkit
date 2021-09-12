namespace ReliableUdp.Contracts
{
    using System;
    using UdpToolkit.Annotations;

    [UdpEvent]
    public class Death
    {
        public Death(
            string nickname,
            Guid roomId)
        {
            Nickname = nickname;
            RoomId = roomId;
        }

        public string Nickname { get; }

        public Guid RoomId { get; }
    }
}