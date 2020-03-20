using System.Net;
using UdpToolkit.Network.Rudp;

namespace UdpToolkit.Network.Peers
{
    public sealed class Peer
    {
        private readonly ReliableChannel _reliableChannel;
        
        public Peer(
            string id,
            IPEndPoint remotePeer, 
            ReliableChannel reliableChannel)
        {
            Id = id;
            RemotePeer = remotePeer;
            _reliableChannel = reliableChannel;
        }

        public ReliableUdpHeader GetReliableHeader() => _reliableChannel.GetReliableHeader();

        public void InsertPacket(uint number) => _reliableChannel.InsertPacket(number);
        
        public string  Id { get; }

        public IPEndPoint RemotePeer { get; }
    }
}
