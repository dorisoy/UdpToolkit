namespace UdpToolkit.Network.Clients
{
    using System;
    using System.Net.Sockets;
    using System.Threading.Tasks;
    using Serilog;
    using UdpToolkit.Network.Packets;
    using UdpToolkit.Network.Protocol;

    public sealed class UdpReceiver : IUdpReceiver
    {
        private readonly UdpClient _receiver;
        private readonly IUdpProtocol _udpProtocol;

        private readonly ILogger _logger = Log.ForContext<UdpReceiver>();

        public UdpReceiver(
            UdpClient receiver,
            IUdpProtocol udpProtocol)
        {
            _receiver = receiver;
            _udpProtocol = udpProtocol;
        }

        public event Action<NetworkPacket> UdpPacketReceived;

        public async Task StartReceiveAsync()
        {
            while (true)
            {
                var result = await _receiver
                    .ReceiveAsync()
                    .ConfigureAwait(false);

                var parseResult = _udpProtocol
                    .TryGetInputPacket(
                        bytes: result.Buffer,
                        ipEndPoint: result.RemoteEndPoint,
                        out var networkPacket);

                if (!parseResult)
                {
                    _logger.Warning("Can't parse received udp packet!");

                    continue;
                }

                UdpPacketReceived?.Invoke(obj: networkPacket);
            }
        }

        public void Dispose()
        {
            _receiver.Dispose();
        }
    }
}
