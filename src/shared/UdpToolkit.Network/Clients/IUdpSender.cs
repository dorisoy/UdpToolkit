namespace UdpToolkit.Network.Clients
{
    using System;
    using System.Net;
    using System.Threading.Tasks;
    using UdpToolkit.Network.Packets;

    public interface IUdpSender : IDisposable
    {
        Task SendAsync(NetworkPacket networkPacket);
    }
}