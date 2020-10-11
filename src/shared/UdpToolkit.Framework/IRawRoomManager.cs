namespace UdpToolkit.Framework
{
    using System;

    public interface IRawRoomManager
    {
        void Apply(
            ushort roomId,
            Func<Peer, bool> condition,
            Action<Peer> action);
    }
}