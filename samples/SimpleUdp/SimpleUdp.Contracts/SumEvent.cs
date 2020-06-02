namespace SimpleUdp.Contracts
{
    using MessagePack;
    using UdpToolkit.Annotations;

    [MessagePackObject]
    [ConsumedEvent(hubId: 0, rpcId: 0, udpChannel: UdpChannel.Udp)]
    public class SumEvent
    {
        [Key(0)]
        public int Sum { get; set; }
    }
}
