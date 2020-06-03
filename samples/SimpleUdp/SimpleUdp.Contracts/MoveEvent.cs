namespace SimpleUdp.Contracts
{
    using MessagePack;
    using UdpToolkit.Annotations;

    [MessagePackObject]
    [ProducedEvent(hubId: 0, rpcId: 2, udpChannel: UdpChannel.Udp)]
    public class MoveEvent
    {
        [Key(0)]
        public byte RoomId { get; set; }

        [Key(1)]
        public float X { get; set; }

        [Key(2)]
        public float Y { get; set; }

        [Key(3)]
        public float Z { get; set; }
    }
}