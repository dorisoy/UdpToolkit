namespace Cubes.Shared.Server
{
    using System;

    public interface IEventHandler<TEvent> : IEventHandler
    {
        event Action<TEvent> OnEvent;

        event Action<Guid> OnAck;

        event Action<Guid> OnTimeout;
    }
}