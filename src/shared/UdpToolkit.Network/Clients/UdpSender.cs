namespace UdpToolkit.Network.Clients
{
    using System.Net.Sockets;
    using System.Threading.Tasks;
    using Serilog;
    using UdpToolkit.Network.Packets;

    public sealed class UdpSender : IUdpSender
    {
        private const int MtuSizeLimit = 3000;
        private readonly ILogger _logger = Log.Logger.ForContext<UdpSender>();
        private readonly UdpClient _sender;

        public UdpSender(
            UdpClient sender)
        {
            _sender = sender;
            _logger.Debug($"{nameof(UdpSender)} - {sender.Client.LocalEndPoint} created");
        }

        public void Dispose()
        {
            _sender.Dispose();
        }

        public async Task SendAsync(NetworkPacket networkPacket)
        {
            var bytes = NetworkPacket.Serialize(networkPacket);

            if (bytes.Length > MtuSizeLimit)
            {
                _logger.Error($"Udp packet oversize mtu limit - {bytes.Length}");

                return;
            }

            _logger.Debug($"Packet from - {_sender.Client.LocalEndPoint} to {networkPacket.IpEndPoint} sended");
            _logger.Debug("Packet sends: {@packet}", networkPacket);

            await _sender
                .SendAsync(bytes, bytes.Length, networkPacket.IpEndPoint)
                .ConfigureAwait(false);
        }
    }
}
