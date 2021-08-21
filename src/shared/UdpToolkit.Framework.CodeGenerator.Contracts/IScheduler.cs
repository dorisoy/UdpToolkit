// ReSharper disable once CheckNamespace
namespace UdpToolkit.Framework.Contracts
{
    using System;

    public interface IScheduler : IDisposable
    {
        void Schedule<TEvent>(
            TEvent @event,
            Guid caller,
            byte channelId,
            int roomId,
            string eventName,
            TimeSpan dueTime,
            BroadcastMode broadcastMode);
    }
}