namespace UdpToolkit.Network.Sockets
{
    using System.Net;
    using UdpToolkit.Logging;

    public sealed class NativeSocketFactory : ISocketFactory
    {
        private readonly IUdpToolkitLoggerFactory _loggerFactory;

        public NativeSocketFactory(
            IUdpToolkitLoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
        }

        public ISocket Create(
            IPEndPoint localEndPoint)
        {
            ISocket nativeSocket = new NativeSocket(_loggerFactory.Create<NativeSocket>());
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