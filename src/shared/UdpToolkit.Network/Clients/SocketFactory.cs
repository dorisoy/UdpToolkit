namespace UdpToolkit.Network.Clients
{
    using System.Net;
    using System.Net.Sockets;
    using System.Runtime.InteropServices;
    using UdpToolkit.Logging;
    using UdpToolkit.Network.Sockets;

    public static class SocketFactory
    {
        private const int SioUdpConnreset = -1744830452;

        public static ISocket Create(
            IPEndPoint localEndPoint,
            IUdpToolkitLoggerFactory loggerFactory)
        {
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            socket.Blocking = false;
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, false);

            // https://stackoverflow.com/questions/38191968/c-sharp-udp-an-existing-connection-was-forcibly-closed-by-the-remote-host
            // https://stackoverflow.com/questions/5116977/how-to-check-the-os-version-at-runtime-e-g-on-windows-or-linux-without-using/47390306#47390306
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                socket.IOControl((IOControlCode)SioUdpConnreset, new byte[] { 0, 0, 0, 0 }, null);
            }

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
