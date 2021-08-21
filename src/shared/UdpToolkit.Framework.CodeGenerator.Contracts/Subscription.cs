// ReSharper disable once CheckNamespace
namespace UdpToolkit.Framework.Contracts
{
    using System;
    using UdpToolkit.Network.Contracts.Sockets;

    public delegate int OnEvent<in TEvent>(
        TEvent @event,
        Guid connectionId,
        IpV4Address ipV4Address,
        byte channelId,
        IRoomManager roomManager,
        IScheduler scheduler);

    public class Subscription<TEvent>
    {
        public Subscription(
            OnEvent<TEvent> onEvent,
            Action<Guid> onTimeout)
        {
            OnEvent = onEvent;
            OnTimeout = onTimeout;
        }

        public OnEvent<TEvent> OnEvent { get; }

        public Action<Guid> OnTimeout { get; }
    }
}