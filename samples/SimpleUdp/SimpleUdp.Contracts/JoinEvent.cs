namespace SimpleUdp.Contracts
{
    using MessagePack;
    using UdpToolkit.Annotations;

    [MessagePackObject]
    [ProducedEvent(hubId: 0, rpcId: 0, udpChannel: UdpChannel.Udp)]
    public class JoinEvent
    {
        [Key(0)]
        public byte RoomId { get; set; }

        [Key(1)]
        public string Nickname { get; set; }
    }
}