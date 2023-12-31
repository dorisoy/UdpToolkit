﻿namespace UdpToolkit.Network.Sockets
{
    using System.Net;
    using System.Net.Sockets;
    using System.Runtime.InteropServices;
    using UdpToolkit.Network.Contracts.Sockets;

    /// <summary>
    /// Abstraction for managed .NET socket creation.
    /// </summary>
    /// <remarks>
    /// Useful if native sockets are not implemented for the target platform.
    /// </remarks>
    public sealed class ManagedSocketFactory : ISocketFactory
    {
        private const int SioUdpConnreset = -1744830452;

        /// <inheritdoc />
        public ISocket Create(
            IpV4Address ipV4Address)
        {
            var socket = new Socket(AddressFamily.InterNetwork, System.Net.Sockets.SocketType.Dgram, ProtocolType.Udp);
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, false);

            // https://stackoverflow.com/questions/38191968/c-sharp-udp-an-existing-connection-was-forcibly-closed-by-the-remote-host
            // https://stackoverflow.com/questions/5116977/how-to-check-the-os-version-at-runtime-e-g-on-windows-or-linux-without-using/47390306#47390306
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                socket.IOControl((IOControlCode)SioUdpConnreset, new byte[] { 0, 0, 0, 0 }, null);
            }

            ISocket managedSocket = new ManagedSocket(socket);

            managedSocket.Bind(ref ipV4Address);
            managedSocket.SetNonBlocking();
            return managedSocket;
        }
    }
}
