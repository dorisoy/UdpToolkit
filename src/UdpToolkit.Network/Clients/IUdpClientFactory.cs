using System.Net;
using System.Net.Sockets;

namespace UdpToolkit.Network.Clients
{
    public interface IUdpClientFactory
    {
        UdpClient Create(IPEndPoint endPoint);
    }
}