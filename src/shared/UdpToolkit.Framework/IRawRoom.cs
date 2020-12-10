namespace UdpToolkit.Framework
{
    using System;
    using System.Threading.Tasks;
    using UdpToolkit.Core;

    public interface IRawRoom : IRoom
    {
        Task Apply(
            Func<Peer, bool> condition,
            Func<Peer, Task> func);
    }
}