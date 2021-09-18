#pragma warning disable SA0001, SA1600
namespace UdpToolkit.Sandbox
{
    using System;

    public interface IMainThreadDispatcher
    {
        void Enqueue(
            Action action);
    }
}
#pragma warning restore SA0001, SA1600