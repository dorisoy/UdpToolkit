namespace UdpToolkit.Network.Clients
{
    using System;
    using System.Net.Sockets;
    using System.Threading.Tasks;
    using Serilog;
    using UdpToolkit.Network.Packets;

    public sealed class UdpReceiver : IUdpReceiver
    {
        private readonly UdpClient _receiver;
        private readonly IUdpProtocol _udpProtocol;
        private readonly TimeSpan _resendPacketTimeout;

        private readonly ILogger _logger = Log.ForContext<UdpReceiver>();

        public UdpReceiver(
            UdpClient receiver,
            IUdpProtocol udpProtocol,
            TimeSpan resendPacketTimeout)
        {
            _receiver = receiver;
            _udpProtocol = udpProtocol;
            _resendPacketTimeout = resendPacketTimeout;
            _logger.Debug($"{nameof(UdpReceiver)} - {receiver.Client.LocalEndPoint} created");
        }

        public event Action<NetworkPacket> UdpPacketReceived;

        public async Task StartReceiveAsync()
        {
            while (true)
            {
                var result = await _receiver
                    .ReceiveAsync()
                    .ConfigureAwait(false);

                var networkPacket = _udpProtocol.Deserialize(
                        bytes: new ArraySegment<byte>(result.Buffer),
                        ipEndPoint: result.RemoteEndPoint,
                        resendPacketTimeout: _resendPacketTimeout);

                _logger.Debug($"Packet from - {result.RemoteEndPoint} to {_receiver.Client.LocalEndPoint} received");
                _logger.Debug("Packet received: {@packet}", networkPacket);

                UdpPacketReceived?.Invoke(networkPacket);
            }
        }

        public void Dispose()
        {
            _receiver.Dispose();
        }
    }
}
