namespace UdpToolkit.Network.Clients
{
    using System.Net;
    using System.Net.Sockets;
    using UdpToolkit.Logging;
    using UdpToolkit.Network.Sockets;

    public static class SocketFactory
    {
        public static ISocket Create(
            IPEndPoint localEndPoint,
            IUdpToolkitLoggerFactory loggerFactory)
        {
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            socket.Blocking = false;
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, false);
            var managedSocket = new ManagedSocket(socket, loggerFactory.Create<ManagedSocket>());
            var to = new IpV4Address
            {
                Address = localEndPoint.Address.ToInt(),
                Port = (ushort)localEndPoint.Port,
            };
            managedSocket.Bind(ref to);
            return managedSocket;
        }
    }
}
