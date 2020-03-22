namespace UdpToolkit.Network.Clients
{
    using System;
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

        public async Task SendAsync(OutputUdpPacket outputUdpPacket)
        {
            foreach (var peer in outputUdpPacket.Peers)
            {
                byte[] networkPacket = null;
                switch (outputUdpPacket.Mode)
                {
                    case UdpMode.Udp:
                        networkPacket = _udpProtocol.GetUdpPacketBytes(
                            frameworkHeader: outputUdpPacket.FrameworkHeader,
                            payload: outputUdpPacket.Payload);

                        break;
                    case UdpMode.ReliableUdp:
                        networkPacket = _udpProtocol.GetReliableUdpPacketBytes(
                            frameworkHeader: outputUdpPacket.FrameworkHeader,
                            reliableUdpHeader: peer.GetReliableHeader(),
                            payload: outputUdpPacket.Payload);

                        break;
                    default:
                        throw new ArgumentOutOfRangeException($"Unsupoorted udp mode - {outputUdpPacket.Mode}!");
                }

                await _sender
                    .SendAsync(networkPacket, networkPacket.Length, peer.RemotePeer)
                    .ConfigureAwait(false);
            }
        }
    }
}
