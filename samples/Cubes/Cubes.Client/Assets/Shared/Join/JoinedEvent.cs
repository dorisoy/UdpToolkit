namespace Shared.Join
{
    using MessagePack;
    using UdpToolkit.Annotations;

    [MessagePackObject]
    [ProducedEvent(0, 0, UdpChannel.Udp)]
    [ConsumedEvent(0, 0, UdpChannel.Udp)]
    public class JoinedEvent
    {
        [Key(0)]
        public string Nickname { get; set; }
    }
}