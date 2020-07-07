namespace UdpToolkit.Network.Clients
{
    using System;
    using System.Net;
    using System.Threading.Tasks;
    using UdpToolkit.Network.Packets;

    public interface IUdpReceiver : IDisposable
    {
        event Action<NetworkPacket> UdpPacketReceived;

        Task StartReceiveAsync();
    }
}
