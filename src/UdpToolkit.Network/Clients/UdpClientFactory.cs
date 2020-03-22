namespace UdpToolkit.Network.Clients
{
    using System.Net;
    using System.Net.Sockets;

    public sealed class UdpClientFactory : IUdpClientFactory
    {
        public UdpClient Create(IPEndPoint endPoint)
        {
            var udpClient = new UdpClient();

            udpClient.Client.Blocking = false;
            udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            udpClient.Client.Bind(endPoint);

            return udpClient;
        }
    }
}
