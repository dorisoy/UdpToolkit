namespace UdpToolkit.Network.Sockets
{
    using System.Net;

    public interface ISocketFactory
    {
        ISocket Create(
            IPEndPoint localEndPoint);
    }
}