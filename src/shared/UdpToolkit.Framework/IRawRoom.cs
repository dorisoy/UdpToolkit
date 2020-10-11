namespace UdpToolkit.Framework
{
    using System;
    using UdpToolkit.Core;

    public interface IRawRoom : IRoom
    {
        void Apply(
            Func<Peer, bool> condition,
            Action<Peer> action);
    }
}