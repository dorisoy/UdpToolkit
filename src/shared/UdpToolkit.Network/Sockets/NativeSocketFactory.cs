namespace UdpToolkit.Network.Sockets
{
    using System.Net.Sockets;
    using UdpToolkit.Network.Contracts.Sockets;

    /// <summary>
    /// Abstraction for native socket creation.
    /// </summary>
    /// <remarks>
    /// Works not for all platforms.
    /// </remarks>
    public sealed class NativeSocketFactory : ISocketFactory
    {
        /// <inheritdoc />
        public ISocket Create(
            IpV4Address ipV4Address)
        {
            ISocket nativeSocket = new NativeSocket();
            var result = nativeSocket.Bind(ref ipV4Address);
            if (result < 0)
            {
                throw new SocketException(result);
            }

            nativeSocket.SetNonBlocking();
            return nativeSocket;
        }
    }
}