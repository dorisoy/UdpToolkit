namespace UdpToolkit.Core
{
    using System;
    using System.Collections.Generic;

    public interface IRoom
    {
        void AddPeer(
            Guid peerId);

        void RemovePeer(
            Guid peerId);

        IEnumerable<Guid> GetPeers();
    }
}