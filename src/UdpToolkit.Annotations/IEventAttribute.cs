namespace UdpToolkit.Annotations
{
    public interface IEventAttribute
    {
        public byte HubId { get; }
        public byte RpcId { get; }
        public UdpChannel UdpChannel { get; }
    }
}