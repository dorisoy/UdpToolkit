namespace UdpToolkit
{
    using System;
    using System.Threading.Tasks;
    using UdpToolkit.Core;

    public interface IRawRoom : IRoom
    {
        Task Apply(
            Func<Guid, bool> condition,
            Func<Guid, Task> func);
    }
}