namespace UdpToolkit.Network.Contracts.Sockets
{
    public interface ISocketFactory
    {
        ISocket Create(
            IpV4Address ipV4Address);
    }
}