namespace UdpToolkit.Network.Peers
{
    using System.Net;
    using UdpToolkit.Network.Rudp;

    public sealed class Peer
    {
        private readonly ReliableUdpChannel _reliableUdpChannel;

        public Peer(
            string id,
            IPEndPoint remotePeer,
            ReliableUdpChannel reliableUdpChannel)
        {
            Id = id;
            RemotePeer = remotePeer;
            _reliableUdpChannel = reliableUdpChannel;
        }

        public string Id { get; }

        public IPEndPoint RemotePeer { get; }

        public ReliableUdpHeader GetReliableHeader() => _reliableUdpChannel.GetReliableHeader();

        public void InsertPacket(uint number) => _reliableUdpChannel.InsertPacket(number);
    }
}
