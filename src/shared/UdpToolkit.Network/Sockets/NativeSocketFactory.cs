namespace UdpToolkit.Network.Sockets
{
    using UdpToolkit.Network.Contracts.Sockets;

    public sealed class NativeSocketFactory : ISocketFactory
    {
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