namespace UdpToolkit.Framework.Contracts
{
    using System;
    using UdpToolkit.Network.Contracts.Sockets;

    /// <summary>
    /// Extensions for creating subscriptions on events, with broadcasting (Server/MasterClient semantic).
    /// </summary>
    public static class HostExtensions
    {
        /// <summary>
        /// Subscribing on the `event`.
        /// </summary>
        /// <param name="host">Instance of host.</param>
        /// <param name="onEvent">Callback for `event`.</param>
        /// <param name="broadcastMode">Broadcast mode.</param>
        /// <param name="onTimeout">Timeout callback.</param>
        /// <typeparam name="TEvent">
        /// Type of user-defined event.
        /// </typeparam>
        /// <exception cref="ArgumentNullException">
        /// If host instance is null.
        /// </exception>
        public static void On<TEvent>(
            this IHost host,
            Func<Guid, IpV4Address, TEvent, int> onEvent,
            BroadcastMode broadcastMode,
            Action<Guid> onTimeout = null)
        {
#pragma warning disable
            if (host == null) throw new ArgumentNullException(nameof(host));
#pragma warning restore

            host.On(new Subscription<TEvent>(
                onEvent: (@event, connectionId, ipV4, channelId, roomManager, scheduler) =>
                {
                    var roomId = onEvent.Invoke(connectionId, ipV4, @event);
                    scheduler.Schedule(
                        caller: connectionId,
                        broadcastMode: broadcastMode,
                        channelId: channelId,
                        roomId: roomId,
                        eventName: typeof(TEvent).Name,
                        dueTime: TimeSpan.FromMilliseconds(0),
                        @event: @event);
                    return roomId;
                },
                onTimeout: onTimeout));
        }

        /// <summary>
        /// Subscribing on the `event`.
        /// </summary>
        /// <param name="host">Instance of host.</param>
        /// <param name="onEvent">Callback for `event`.</param>
        /// <param name="onTimeout">Timeout callback.</param>
        /// <typeparam name="TInEvent">Type of user-defined input event.</typeparam>
        /// <typeparam name="TOutEvent">Type of user-defined output event.</typeparam>
        /// <exception cref="ArgumentNullException">If host instance is null.</exception>
        public static void On<TInEvent, TOutEvent>(
            this IHost host,
            Func<Guid, IpV4Address, TInEvent, IRoomManager, OutEvent<TOutEvent>> onEvent,
            Action<Guid> onTimeout = null)
        {
#pragma warning disable
            if (host == null) throw new ArgumentNullException(nameof(host));
#pragma warning restore

            host.On(new Subscription<TInEvent>(
                onEvent: (@event, connectionId, ipV4, channelId, roomManager, scheduler) =>
                {
                    var outEvent = onEvent.Invoke(connectionId, ipV4, @event, roomManager);
                    scheduler.Schedule(
                        caller: connectionId,
                        broadcastMode: outEvent.BroadcastMode,
                        channelId: outEvent.ChannelId,
                        roomId: outEvent.RoomId,
                        eventName: typeof(TOutEvent).Name,
                        dueTime: TimeSpan.FromMilliseconds(outEvent.DelayInMs),
                        @event: outEvent.Event);

                    return outEvent.RoomId;
                },
                onTimeout: onTimeout));
        }

        /// <summary>
        /// Subscribing on the `event`.
        /// </summary>
        /// <param name="host">Instance of host.</param>
        /// <param name="onEvent">Callback for `event`.</param>
        /// <param name="broadcastMode">Broadcast mode.</param>
        /// <param name="onTimeout">Timeout callback.</param>
        /// <typeparam name="TEvent">
        /// Type of user-defined event.
        /// </typeparam>
        /// <exception cref="ArgumentNullException">
        /// If host instance is null.
        /// </exception>
        public static void On<TEvent>(
            this IHost host,
            Func<Guid, IpV4Address, TEvent, IRoomManager, int> onEvent,
            BroadcastMode broadcastMode,
            Action<Guid> onTimeout = null)
        {
#pragma warning disable
            if (host == null) throw new ArgumentNullException(nameof(host));
#pragma warning restore

            host.On(new Subscription<TEvent>(
                onEvent: (@event, connectionId, ipV4, channelId, roomManager, scheduler) =>
                {
                    var roomId = onEvent.Invoke(connectionId, ipV4, @event, roomManager);
                    scheduler.Schedule(
                        caller: connectionId,
                        broadcastMode: broadcastMode,
                        channelId: channelId,
                        roomId: roomId,
                        eventName: typeof(TEvent).Name,
                        dueTime: TimeSpan.FromMilliseconds(0),
                        @event: @event);

                    return roomId;
                },
                onTimeout: onTimeout));
        }
    }
}
