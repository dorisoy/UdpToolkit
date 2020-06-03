namespace UdpToolkit.Network.Clients
{
    using System.Net.Sockets;
    using System.Threading.Tasks;
    using UdpToolkit.Network.Packets;
    using UdpToolkit.Network.Protocol;

    public sealed class UdpSender : IUdpSender
    {
        private readonly IUdpProtocol _udpProtocol;
        private readonly UdpClient _sender;

        public UdpSender(
            UdpClient sender,
            IUdpProtocol udpProtocol)
        {
            _sender = sender;
            _udpProtocol = udpProtocol;
        }

        public void Dispose()
        {
            _sender.Dispose();
        }

        public async Task SendAsync(NetworkPacket networkPacket)
        {
            var bytes = _udpProtocol.GetBytes(networkPacket, default); // TODO remove default

            await _sender
                .SendAsync(bytes, bytes.Length, networkPacket.IpEndPoint)
                .ConfigureAwait(false);
        }
    }
}
