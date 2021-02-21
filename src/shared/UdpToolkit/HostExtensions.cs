namespace UdpToolkit
{
    using System;
    using UdpToolkit.Core;
    using UdpToolkit.Core.ProtocolEvents;
    using UdpToolkit.Network.Channels;

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
                    onEvent: (bytes, peerId, serializer, roomManager, scheduler) =>
                    {
                        var l = bytes.Length;
                        try
                        {
                            var @event = serializer.Deserialize<TEvent>(new ArraySegment<byte>(bytes));
                            return onEvent.Invoke(peerId, @event);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                            throw;
                        }
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
            Action<Guid> onTimeout = null,
            ScheduledCall<TEvent> scheduleCall = null)
        {
#pragma warning disable
            if (host == null) throw new ArgumentNullException(nameof(host));
#pragma warning restore

            host.OnCore(
                subscription: new Subscription(
                    onProtocolEvent: null,
                    broadcastMode: broadcastMode,
                    onEvent: (bytes, peerId, serializer, roomManager, scheduler) =>
                    {
                        var @event = serializer.Deserialize<TEvent>(new ArraySegment<byte>(bytes));
                        var roomId = onEvent.Invoke(peerId, @event, roomManager);
                        if (scheduleCall != null)
                        {
                            scheduler.Schedule(roomId, scheduleCall.TimerId, scheduleCall.Delay, () => scheduleCall.Action(peerId, @event, roomManager));
                        }

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
                    onProtocolEvent: (bytes, peerId, serializer) =>
                    {
                        var @event = ProtocolEvent<TEvent>.Deserialize(bytes);
                        onProtocolEvent?.Invoke(peerId, @event);
                    },
                    broadcastMode: BroadcastMode.Caller,
                    onAck: onAck,
                    onTimeout: onTimeout),
                hookId: (byte)protocolHookId);
        }
    }
}
