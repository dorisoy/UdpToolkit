namespace UdpToolkit.Network.Clients
{
    using System.Net;
    using System.Net.Sockets;

    public static class SocketFactory
    {
        public static Socket Create(
            IPEndPoint localEndPoint)
        {
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            socket.Blocking = true;
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, false);
            socket.Bind(localEndPoint);
            return socket;
        }
    }
}
