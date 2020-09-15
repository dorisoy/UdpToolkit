namespace UdpToolkit.Core
{
    using System;
    using System.Collections.Generic;

    public interface IRoom
    {
        ushort RoomId { get; }

        int Size { get; }

        void AddPeer(Guid peerId);

        void RemovePeer(Guid peerId);

        IPeer GetPeer(Guid peerId);

        IEnumerable<IPeer> GetPeers();
    }
}