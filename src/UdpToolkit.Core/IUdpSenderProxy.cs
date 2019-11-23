namespace UdpToolkit.Core
{
    public interface IUdpSenderProxy
    {
        void Publish(OutputUdpPacket outputUdpPacket);
    }
}