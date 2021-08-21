namespace UdpToolkit.Framework.Contracts
{
    using System;

    public interface IHost : IDisposable
    {
        IHostClient HostClient { get; }

        void Run();

        void On<TEvent>(
            Subscription<TEvent> subscription);
    }
}
