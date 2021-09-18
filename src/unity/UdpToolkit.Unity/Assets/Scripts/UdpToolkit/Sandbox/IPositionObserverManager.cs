#pragma warning disable SA0001, SA1600
namespace UdpToolkit.Sandbox
{
    using System;

    public interface IPositionObserverManager
    {
        void Add(
            Guid playerId,
            IPositionObserver positionObserver);

        bool TryGet(
            Guid playerId,
            out IPositionObserver positionObserver);
    }
}
#pragma warning restore SA0001, SA1600