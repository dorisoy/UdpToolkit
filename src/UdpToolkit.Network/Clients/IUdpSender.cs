using System;
using System.Threading.Tasks;
using UdpToolkit.Network.Packets;
using UdpToolkit.Network.Peers;

namespace UdpToolkit.Network.Clients
{
    public interface IUdpSender : IDisposable
    {
        Task Send(OutputUdpPacket outputUdpPacket);
    }
}