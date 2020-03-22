namespace UdpToolkit.Network.Clients
{
    using System;
    using System.Threading.Tasks;
    using UdpToolkit.Network.Packets;

    public interface IUdpReceiver : IDisposable
    {
        event Action<InputUdpPacket> UdpPacketReceived;

        Task StartReceiveAsync();
    }
}
