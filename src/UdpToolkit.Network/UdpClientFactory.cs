using System.Net;
using System.Net.Sockets;
using UdpToolkit.Core;

namespace UdpToolkit.Network
{
    public class UdpClientFactory : IUdpClientFactory
    {
        public UdpClient Create(IPEndPoint endPoint)
        {
            var udpClient = new UdpClient();
            
            udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            udpClient.Client.Bind(endPoint);
            
            return udpClient;
        }
    }
}
