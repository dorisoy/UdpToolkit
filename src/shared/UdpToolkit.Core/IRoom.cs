namespace UdpToolkit.Core
{
    using System;
    using System.Collections.Generic;

    public interface IRoom
    {
        void AddPeer(
            IPeer peer);

        void RemovePeer(
            Guid peerId);

        IEnumerable<Guid> GetPeers();
    }
}