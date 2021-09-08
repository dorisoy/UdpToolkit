namespace ReliableUdp.Contracts
{
    using UdpToolkit.Annotations;

    [UdpEvent]
    public class JoinEvent
    {
        public JoinEvent(
            int roomId,
            string nickname)
        {
            RoomId = roomId;
            Nickname = nickname;
        }

        public int RoomId { get; set; }

        public string Nickname { get; set; }
    }
}