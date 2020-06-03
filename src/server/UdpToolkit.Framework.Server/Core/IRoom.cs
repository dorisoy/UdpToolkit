namespace UdpToolkit.Framework.Server.Core
{
    using System;
    using System.Collections.Generic;
    using UdpToolkit.Network.Peers;

    public interface IRoom
    {
        ushort RoomId { get; }

        int Size { get; }

        void AddPeer(Peer peer);

        void RemovePeer(Guid peerId);

        Peer GetPeer(Guid peerId);

        IEnumerable<Peer> GetPeers();
    }
}