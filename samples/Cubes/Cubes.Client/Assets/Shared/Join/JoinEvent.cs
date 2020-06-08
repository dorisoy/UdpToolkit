namespace Shared.Join
{
    using MessagePack;
    using UdpToolkit.Annotations;

    [MessagePackObject]
    [ProducedEvent(0, 0, UdpChannel.Udp)]
    [ConsumedEvent(0, 0, UdpChannel.Udp)]
    public class JoinEvent
    {
        [Key(0)]
        public byte RoomId { get; set; }

        [Key(1)]
        public string Nickname { get; set; }
    }
}