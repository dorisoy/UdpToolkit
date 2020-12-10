namespace UdpToolkit.Core
{
    using System;

    public interface IRoom
    {
        void AddPeer(IPeer peer);

        void RemovePeer(Guid peerId);
    }
}