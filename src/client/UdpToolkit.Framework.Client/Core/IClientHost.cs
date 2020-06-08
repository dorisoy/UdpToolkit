namespace UdpToolkit.Framework.Client.Core
{
    using System;
    using UdpToolkit.Core;

    public interface IClientHost : IHost
    {
        void On<TEvent>(Action<TEvent> handler);

        void Publish<TEvent>(TEvent @event);
    }
}