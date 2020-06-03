namespace SimpleUdp.Contracts
{
    using MessagePack;
    using UdpToolkit.Annotations;

    [MessagePackObject]
    [ProducedEvent(hubId: 0, rpcId: 1, udpChannel: UdpChannel.Udp)]
    public class LeaveEvent
    {
        [Key(0)]
        public byte RoomId { get; set; }

        [Key(1)]
        public string Nickname { get; set; }
    }
}