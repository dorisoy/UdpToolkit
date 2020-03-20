using System;
using System.Threading.Tasks;
using UdpToolkit.Network.Packets;

namespace UdpToolkit.Network.Clients
{
    public interface IUdpReceiver : IDisposable
    {
        Task StartReceiveAsync();

        event Action<InputUdpPacket> UdpPacketReceived;
    }
}
