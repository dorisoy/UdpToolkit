namespace UdpToolkit.Network.Contracts.Clients
{
    using UdpToolkit.Network.Contracts.Sockets;

    public interface IUdpClientFactory
    {
        IUdpClient Create(
            IpV4Address ipV4Address);
    }
}