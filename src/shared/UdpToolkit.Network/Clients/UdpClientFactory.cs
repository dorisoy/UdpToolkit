namespace UdpToolkit.Network.Clients
{
    using System.Net;
    using System.Net.Sockets;

    public static class UdpClientFactory
    {
        public static UdpClient Create(
            IPEndPoint localEndPoint)
        {
            var udpClient = new UdpClient();

            udpClient.Client.Blocking = false;
            udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            udpClient.Client.Bind(localEndPoint);
            return udpClient;
        }
    }
}
