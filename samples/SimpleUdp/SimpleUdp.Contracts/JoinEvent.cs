namespace SimpleUdp.Contracts
{
    using MessagePack;
    using UdpToolkit.Annotations;

    [MessagePackObject]
    [ProducedEvent(0, 0, UdpChannel.Udp)]
    [ConsumedEvent(0, 0, UdpChannel.Udp)]

    public class JoinEvent
    {
        [Key(0)]
        public byte RoomId { get; set; } = 200;

        [Key(1)]
        public int Index { get; set; } = 777;
    }
}