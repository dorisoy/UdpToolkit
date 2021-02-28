namespace UdpToolkit.Core
{
    using System;
    using System.Net;
    using System.Threading.Tasks;

    public interface IHost : IDisposable
    {
        IHostClient HostClient { get; }

        IScheduler Scheduler { get; }

        Task RunAsync();

        void Stop();

        void OnCore(
            byte hookId,
            Subscription subscription);

        void SendCore<TEvent>(
            TEvent @event,
            Guid caller,
            int roomId,
            byte hookId,
            UdpMode udpMode,
            BroadcastMode broadcastMode);
    }
}
