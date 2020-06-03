namespace SimpleUdp.Contracts
{
    using MessagePack;
    using UdpToolkit.Annotations;

    [MessagePackObject]
    [ConsumedEvent(hubId: 0, rpcId: 1, udpChannel: UdpChannel.Udp)]
    public class LeavedEvent
    {
        public string Nickname { get; set; }
    }
}