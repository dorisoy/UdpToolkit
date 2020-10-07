namespace UdpToolkit.Core
{
    using System;
    using System.Threading.Tasks;

    public interface IHost : IDisposable
    {
        IServerHostClient ServerHostClient { get; }

        Task RunAsync();

        void Stop();

        void OnCore(Subscription subscription, byte hookId);
    }
}
