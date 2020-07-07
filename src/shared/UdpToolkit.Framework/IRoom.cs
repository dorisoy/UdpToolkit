namespace UdpToolkit.Framework
{
    using System;
    using System.Collections.Generic;

    public interface IRoom
    {
        ushort RoomId { get; }

        int Size { get; }

        void AddPeer(Guid peerId);

        void RemovePeer(Guid peerId);

        Peer GetPeer(Guid peerId);

        IEnumerable<Peer> GetPeers();
    }
}