namespace UdpToolkit.Network.Clients
{
    using System;
    using System.Net.Sockets;
    using System.Threading.Tasks;
    using UdpToolkit.Network.Packets;
    using UdpToolkit.Network.Protocol;

    public sealed class UdpReceiver : IUdpReceiver
    {
        private readonly UdpClient _receiver;
        private readonly IUdpProtocol _udpProtocol;

        public UdpReceiver(
            UdpClient receiver,
            IUdpProtocol udpProtocol)
        {
            _receiver = receiver;
            _udpProtocol = udpProtocol;
        }

        public event Action<InputUdpPacket> UdpPacketReceived;

        public async Task StartReceiveAsync()
        {
            while (true)
            {
                var result = await _receiver
                    .ReceiveAsync()
                    .ConfigureAwait(false);

                var parseResult = _udpProtocol
                    .TryParseProtocol(
                        packet: result.Buffer,
                        out var packetType,
                        out var frameworkHeader,
                        out var reliableUdpHeader,
                        out var payload);

                if (!parseResult)
                {
                    continue;
                }

                UdpPacketReceived?.Invoke(
                    obj: new InputUdpPacket(
                        hubId: frameworkHeader.HubId,
                        rpcId: frameworkHeader.RpcId,
                        scopeId: frameworkHeader.ScopeId,
                        peerId: result.RemoteEndPoint.ToString(),
                        payload: payload,
                        remotePeer: result.RemoteEndPoint));
            }
        }

        public void Dispose()
        {
            _receiver.Dispose();
        }
    }
}
