namespace UdpToolkit.Network.Clients
{
    using System.Net;
    using System.Net.Sockets;

    public interface IUdpClientFactory
    {
        UdpClient Create(
            IPEndPoint localEndPoint);
    }
}