// ReSharper disable once CheckNamespace
namespace UdpToolkit.Framework.Contracts
{
    using System;
    using UdpToolkit.Network.Contracts.Sockets;

    /// <summary>
    /// Delegate represents user-define subscription on the event.
    /// </summary>
    /// <typeparam name="TEvent">
    /// Type of user-defined event.
    /// </typeparam>
    /// <param name="event">Instance of user-defined event.</param>
    /// <param name="connectionId">Connection identifier.</param>
    /// <param name="ipV4Address">Ip address of connection.</param>
    /// <param name="channelId">Channel identifier.</param>
    /// <param name="roomManager">Instance of room manager.</param>
    /// <param name="scheduler">Instance of scheduler.</param>
    /// <returns>
    /// Room identifier.
    /// </returns>
    public delegate int OnEvent<in TEvent>(
        TEvent @event,
        Guid connectionId,
        IpV4Address ipV4Address,
        byte channelId,
        IRoomManager roomManager,
        IScheduler scheduler);

    /// <summary>
    /// Subscription on event.
    /// </summary>
    /// <typeparam name="TEvent">
    /// Type of user-defined event.
    /// </typeparam>
    public class Subscription<TEvent>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Subscription{TEvent}"/> class.
        /// </summary>
        /// <param name="onEvent">Callback raised on event received.</param>
        /// <param name="onTimeout">Callback raised on event expired by timeout (ack dropped or not received a due period of time).</param>
        public Subscription(
            OnEvent<TEvent> onEvent,
            Action<Guid> onTimeout)
        {
            OnEvent = onEvent;
            OnTimeout = onTimeout;
        }

        /// <summary>
        /// Gets on event callback.
        /// </summary>
        public OnEvent<TEvent> OnEvent { get; }

        /// <summary>
        /// Gets on timeout callback.
        /// </summary>
        public Action<Guid> OnTimeout { get; }
    }
}