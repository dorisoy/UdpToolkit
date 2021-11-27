namespace UdpToolkit.Benchmarks.Fakes
{
    using UdpToolkit.Network.Contracts.Sockets;

    internal class FakeSocketFactory : ISocketFactory
    {
        public ISocket Create(IpV4Address ipV4Address)
        {
            return new FakeSocket(ipV4Address);
        }
    }
}