namespace UdpToolkit.Framework
{
    using System;

    public interface IRawRoomManager
    {
        void Remove(
            ushort roomId,
            Peer peer);

        void Apply(
            ushort roomId,
            Func<Peer, bool> condition,
            Action<Peer> action);
    }
}