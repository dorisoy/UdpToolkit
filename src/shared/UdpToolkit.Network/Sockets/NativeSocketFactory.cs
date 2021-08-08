namespace UdpToolkit.Network.Sockets
{
    using System.Net;
    using UdpToolkit.Network.Contracts.Sockets;

    public sealed class NativeSocketFactory : ISocketFactory
    {
        public ISocket Create(
            IPEndPoint localEndPoint)
        {
            ISocket nativeSocket = new NativeSocket();
            var to = new IpV4Address
            {
                Address = localEndPoint.Address.ToInt(),
                Port = (ushort)localEndPoint.Port,
            };

            nativeSocket.Bind(ref to);
            nativeSocket.SetNonBlocking();
            return nativeSocket;
        }
    }
}