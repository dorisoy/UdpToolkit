namespace UdpToolkit.Framework
{
    using System;
    using UdpToolkit.Core;
    using UdpToolkit.Core.ProtocolEvents;
    using UdpToolkit.Network.Channels;

    public static class HostExtensions
    {
        public static void On<TEvent>(
            this IHost host,
            Action<Guid, TEvent> onEvent,
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
                    broadcastMode: broadcastMode,
                    onEvent: (bytes, peerId, serializer, roomManager) =>
                    {
                        var @event = serializer.Deserialize<TEvent>(new ArraySegment<byte>(bytes));
                        onEvent?.Invoke(peerId, @event);
                    },
                    onAck: onAck,
                    onTimeout: onTimeout),
                hookId: hookId);
        }

        public static void OnProtocol<TEvent>(
            this IHost host,
            Action<Guid, TEvent> onEvent,
            ProtocolHookId protocolHookId,
            Action<Guid> onAck = null,
            Action<Guid> onTimeout = null)
        where TEvent : IProtocolEvent
        {
#pragma warning disable
            if (host == null) throw new ArgumentNullException(nameof(host));
#pragma warning restore

            host.OnCore(
                subscription: new Subscription(
                    broadcastMode: BroadcastMode.Caller,
                    onEvent: (bytes, peerId, serializer, roomManager) =>
                    {
                        var @event = serializer.DeserializeContractLess<TEvent>(new ArraySegment<byte>(bytes));
                        onEvent?.Invoke(peerId, @event);
                    },
                    onAck: onAck,
                    onTimeout: onTimeout),
                hookId: (byte)protocolHookId);
        }

        public static void On<TEvent>(
            this IHost host,
            byte hookId,
            BroadcastMode broadcastMode,
            Action<Guid, TEvent, IRoomManager> onEvent = null,
            Action<Guid> onAck = null,
            Action<Guid> onTimeout = null)
        {
#pragma warning disable
            if (host == null) throw new ArgumentNullException(nameof(host));
#pragma warning restore

            host.OnCore(
                subscription: new Subscription(
                    broadcastMode: broadcastMode,
                    onEvent: (bytes, peerId, serializer, roomManager) =>
                    {
                        var @event = serializer.Deserialize<TEvent>(new ArraySegment<byte>(bytes));
                        onEvent?.Invoke(peerId, @event, roomManager);
                    },
                    onAck: onAck,
                    onTimeout: onTimeout),
                hookId: hookId);
        }
    }
}
