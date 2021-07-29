namespace UdpToolkit
{
    using System;
    using UdpToolkit.Core;
    using UdpToolkit.Network.Channels;
    using UdpToolkit.Network.Protocol;

    public static class HostExtensions
    {
        public static void On<TEvent>(
            this IHost host,
            Func<Guid, TEvent, int> onEvent,
            BroadcastMode broadcastMode,
            byte hookId,
            Action<Guid> onAck = null,
            Action<Guid> onTimeout = null)
        {
#pragma warning disable
            if (host == null) throw new ArgumentNullException(nameof(host));
#pragma warning restore

            host.OnCore(
                subscription: new Subscription(
                    onProtocolEvent: null,
                    broadcastMode: broadcastMode,
                    onEvent: (bytes, connectionId, serializer, roomManager, scheduler) =>
                    {
                        var @event = serializer.Deserialize<TEvent>(new ArraySegment<byte>(bytes));
                        return onEvent.Invoke(connectionId, @event);
                    },
                    onAck: onAck,
                    onTimeout: onTimeout),
                hookId: hookId);
        }

        public static void On<TRequest, TResponse>(
            this IHost host,
            Func<Guid, TRequest, IRoomManager, (int roomId, TResponse response, int delayInMs, BroadcastMode broadcastMode, UdpMode uppMode)> onEvent,
            byte hookId,
            BroadcastMode broadcastMode,
            Action<Guid> onAck = null,
            Action<Guid> onTimeout = null)
        {
#pragma warning disable
            if (host == null) throw new ArgumentNullException(nameof(host));
#pragma warning restore

            host.OnCore(
                subscription: new Subscription(
                    onProtocolEvent: null,
                    broadcastMode: broadcastMode,
                    onEvent: (bytes, connectionId, serializer, roomManager, scheduler) =>
                    {
                        var @event = serializer.Deserialize<TRequest>(new ArraySegment<byte>(bytes));
                        var tuple = onEvent.Invoke(connectionId, @event, roomManager);

                        if (tuple.delayInMs > 0)
                        {
                            scheduler.Schedule(
                                roomId: tuple.roomId,
                                timerId: hookId,
                                dueTime: TimeSpan.FromMilliseconds(tuple.delayInMs),
                                action: () =>
                                {
                                    host.SendCore(
                                        @event: tuple.response,
                                        caller: connectionId,
                                        roomId: tuple.roomId,
                                        hookId: hookId,
                                        udpMode: tuple.uppMode,
                                        broadcastMode: tuple.broadcastMode);
                            });
                        }
                        else
                        {
                            host.SendCore(
                                @event: tuple.response,
                                caller: connectionId,
                                roomId: tuple.roomId,
                                hookId: hookId,
                                udpMode: tuple.uppMode,
                                broadcastMode: tuple.broadcastMode);
                        }

                        return tuple.roomId;
                    },
                    onAck: onAck,
                    onTimeout: onTimeout),
                hookId: hookId);
        }

        public static void On<TEvent>(
            this IHost host,
            Func<Guid, TEvent, IRoomManager, int> onEvent,
            BroadcastMode broadcastMode,
            byte hookId,
            Action<Guid> onAck = null,
            Action<Guid> onTimeout = null)
        {
#pragma warning disable
            if (host == null) throw new ArgumentNullException(nameof(host));
#pragma warning restore

            host.OnCore(
                subscription: new Subscription(
                    onProtocolEvent: null,
                    broadcastMode: broadcastMode,
                    onEvent: (bytes, connectionId, serializer, roomManager, scheduler) =>
                    {
                        var @event = serializer.Deserialize<TEvent>(new ArraySegment<byte>(bytes));
                        var roomId = onEvent.Invoke(connectionId, @event, roomManager);

                        return roomId;
                    },
                    onAck: onAck,
                    onTimeout: onTimeout),
                hookId: hookId);
        }

        public static void OnProtocol<TEvent>(
            this IHost host,
            Action<Guid, TEvent> onProtocolEvent,
            Action<Guid> onAck,
            Action<Guid> onTimeout,
            ProtocolHookId protocolHookId)
        where TEvent : ProtocolEvent<TEvent>, new()
        {
#pragma warning disable
            if (host == null) throw new ArgumentNullException(nameof(host));
#pragma warning restore

            host.OnCore(
                subscription: new Subscription(
                    onEvent: null,
                    onProtocolEvent: (bytes, connectionId, serializer) =>
                    {
                        var @event = ProtocolEvent<TEvent>.Deserialize(bytes);
                        onProtocolEvent?.Invoke(connectionId, @event);
                    },
                    broadcastMode: BroadcastMode.Caller,
                    onAck: onAck,
                    onTimeout: onTimeout),
                hookId: (byte)protocolHookId);
        }
    }
}
