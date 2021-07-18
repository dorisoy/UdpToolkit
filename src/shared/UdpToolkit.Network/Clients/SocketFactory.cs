namespace UdpToolkit.Network.Clients
{
    using System.Net;
    using System.Net.Sockets;
    using UdpToolkit.Network.Sockets;

    public static class SocketFactory
    {
        public static ISocket Create(
            IPEndPoint localEndPoint)
        {
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            socket.Blocking = false;
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, false);
            var managedSocket = new ManagedSocket(socket);
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
