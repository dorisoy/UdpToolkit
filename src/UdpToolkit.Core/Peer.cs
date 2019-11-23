using System;
using System.Net;

namespace UdpToolkit.Core
{
    public sealed class Peer
    {
        public Peer(
            IPEndPoint remotePeer)
        {
            //TODO do not use user ip address as peerId
            Id = remotePeer.ToString();
            RemotePeer = remotePeer;
        }

        public string  Id { get; }

        public IPEndPoint RemotePeer { get; }
    }
}
