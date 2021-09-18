#pragma warning disable SA0001, SA1600
namespace UdpToolkit.Sandbox
{
    using System;
    using System.Collections.Generic;

    public sealed class PositionObserverManager : IPositionObserverManager
    {
        private readonly Dictionary<Guid, IPositionObserver> _observers;

        public PositionObserverManager(
            Dictionary<Guid, IPositionObserver> observers)
        {
            _observers = observers;
        }

        public void Add(
            Guid playerId,
            IPositionObserver positionObserver)
        {
            _observers[playerId] = positionObserver;
        }

        public bool TryGet(
            Guid playerId,
            out IPositionObserver positionObserver)
        {
            return _observers.TryGetValue(playerId, out positionObserver);
        }
    }
}
#pragma warning restore SA0001, SA1600