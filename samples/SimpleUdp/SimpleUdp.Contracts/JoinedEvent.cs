namespace SimpleUdp.Contracts
{
    using MessagePack;
    using UdpToolkit.Annotations;

    [MessagePackObject]
    [ConsumedEvent(hubId: 0, rpcId: 0, udpChannel: UdpChannel.Udp)]
    public class JoinedEvent
    {
        [Key(0)]
        public string Nickname { get; set; }
    }
}