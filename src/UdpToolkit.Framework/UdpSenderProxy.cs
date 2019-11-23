using UdpToolkit.Core;
using UdpToolkit.Network;

namespace UdpToolkit.Framework
{
    public sealed class UdpSenderProxy : IUdpSenderProxy
    {
        private readonly AsyncQueue<OutputUdpPacket> _outputQueue;

        public UdpSenderProxy(AsyncQueue<OutputUdpPacket> outputQueue)
        {
            _outputQueue = outputQueue;
        }
        
        public void Publish(OutputUdpPacket outputUdpPacket)
        {
            _outputQueue.Publish(outputUdpPacket);
        }
    }
}