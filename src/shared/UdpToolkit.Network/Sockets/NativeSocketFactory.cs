namespace UdpToolkit.Network.Sockets
{
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

            nativeSocket.Bind(ref ipV4Address);
            nativeSocket.SetNonBlocking();
            return nativeSocket;
        }
    }
}