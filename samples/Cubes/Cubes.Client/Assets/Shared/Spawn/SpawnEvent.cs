namespace Shared.Spawn
{
    using MessagePack;
    using UdpToolkit.Annotations;

    [MessagePackObject]
    [ProducedEvent(0, 1, UdpChannel.Udp)]
    [ConsumedEvent(0, 1, UdpChannel.Udp)]
    public class SpawnEvent
    {
        [Key(0)]
        public byte RoomId { get; set; }

        [Key(1)]
        public string Nickname { get; set; }
    }
}