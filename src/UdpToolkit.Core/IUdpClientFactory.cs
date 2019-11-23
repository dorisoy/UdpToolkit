using System.Net;
using System.Net.Sockets;

namespace UdpToolkit.Core
{
    public interface IUdpClientFactory
    {
        UdpClient Create(IPEndPoint endPoint);
    }
}