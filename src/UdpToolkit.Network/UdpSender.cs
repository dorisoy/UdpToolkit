using System.Net.Sockets;
using System.Threading.Tasks;
using UdpToolkit.Core;

namespace UdpToolkit.Network
{
    public sealed class UdpSender : IUdpSender
    {
        private readonly AsyncQueue<OutputUdpPacket> _outputQueue;
        private readonly UdpClient _sender;

        public UdpSender(
            AsyncQueue<OutputUdpPacket> outputQueue,
            UdpClient sender)

        {
            _outputQueue = outputQueue;
            _sender = sender;
        }

        public void Dispose()
        {
            _sender.Dispose();
        }

        public Task Send(OutputUdpPacket outputUdpPacket)
        {
            _outputQueue.Publish(outputUdpPacket);
            
            return Task.CompletedTask;
        }

        public async Task StartSending()
        {
            foreach (var packet in _outputQueue.Consume())
            {
                foreach (var peer in packet.Peers)
                {
                    await _sender.SendAsync(packet.Bytes, packet.Bytes.Length, peer.RemotePeer);   
                }
            }
        }
    }
}
