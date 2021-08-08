namespace UdpToolkit.Network.Contracts.Sockets
{
    using System.Net;

    public interface ISocketFactory
    {
        ISocket Create(
            IPEndPoint localEndPoint);
    }
}