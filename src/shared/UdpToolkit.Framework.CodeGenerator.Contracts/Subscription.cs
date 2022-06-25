// ReSharper disable once CheckNamespace
namespace UdpToolkit.Framework.Contracts
{
    using System;
    using UdpToolkit.Network.Contracts.Sockets;

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
        /// <param name="broadcastMode">Broadcast mode.</param>
        public Subscription(
            Func<Guid, IpV4Address, TEvent, Guid> onEvent,
            Action onTimeout,
            BroadcastMode broadcastMode = default)
        {
            OnEvent = onEvent;
            OnTimeout = onTimeout;
            BroadcastMode = broadcastMode;
        }

        /// <summary>
        /// Gets on event callback.
        /// </summary>
        public Func<Guid, IpV4Address, TEvent, Guid> OnEvent { get; }

        /// <summary>
        /// Gets on timeout callback.
        /// </summary>
        public Action OnTimeout { get; }

        /// <summary>
        /// Gets broadcast mode.
        /// </summary>
        public BroadcastMode BroadcastMode { get; }
    }
}