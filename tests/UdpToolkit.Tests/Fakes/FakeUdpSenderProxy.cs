using UdpToolkit.Core;

namespace UdpToolkit.Tests.Fakes
{
    public class FakeUdpSenderProxy : IUdpSenderProxy
    {
        public void Publish(OutputUdpPacket outputUdpPacket)
        {
            throw new System.NotImplementedException();
        }
    }
}
