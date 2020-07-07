namespace UdpToolkit.Network.Clients
{
    using System.Net.Sockets;
    using System.Threading.Tasks;
    using Serilog;
    using UdpToolkit.Network.Packets;
    using UdpToolkit.Network.Protocol;

    public sealed class UdpSender : IUdpSender
    {
        private readonly ILogger _logger = Log.Logger.ForContext<UdpSender>();
        private readonly IUdpProtocol _udpProtocol;
        private readonly UdpClient _sender;

        public UdpSender(
            UdpClient sender,
            IUdpProtocol udpProtocol)
        {
            _sender = sender;
            _udpProtocol = udpProtocol;
            _logger.Debug($"{nameof(UdpSender)} - {sender.Client.LocalEndPoint} created");
        }

        public void Dispose()
        {
            _sender.Dispose();
        }

        public async Task SendAsync(NetworkPacket networkPacket)
        {
            var bytes = _udpProtocol.GetBytes(networkPacket: networkPacket);

            if (bytes.Length > Consts.Mtu)
            {
                _logger.Error($"Udp packet oversize mtu limit - {bytes.Length}");

                return;
            }

            _logger.Debug($"Packet from - {_sender.Client.LocalEndPoint} to {networkPacket.IpEndPoint} sended");

            await _sender
                .SendAsync(bytes, bytes.Length, networkPacket.IpEndPoint)
                .ConfigureAwait(false);
        }
    }
}
