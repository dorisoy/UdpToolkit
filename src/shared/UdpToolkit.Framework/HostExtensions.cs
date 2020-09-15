namespace UdpToolkit.Framework
{
    using System;
    using UdpToolkit.Core;
    using UdpToolkit.Network.Channels;

    public static class HostExtensions
    {
        public static void Publish<TEvent>(
            this IHost host,
            Func<IDatagramBuilder, Datagram<TEvent>> datagramFactory,
            UdpMode udpMode)
        {
            host.PublishCore(datagramFactory, udpMode);
        }

        public static void On<TEvent>(
            this IHost host,
            Action<Guid, TEvent> handler,
            byte hookId)
        {
#pragma warning disable
            if (host == null) throw new ArgumentNullException(nameof(host));
#pragma warning restore

            host.OnCore<TEvent>(
                subscription: (bytes, peerId, serializer, roomManager, builder, udpMode) =>
                {
                    var @event = serializer.Deserialize<TEvent>(new ArraySegment<byte>(bytes));
                    handler(peerId, @event);
                },
                hookId: hookId);
        }

        public static void On<TEvent>(
            this IHost host,
            Action<Guid, TEvent> handler,
            PacketType packetType)
        {
#pragma warning disable
            if (host == null) throw new ArgumentNullException(nameof(host));
#pragma warning restore

            host.OnCore<TEvent>(
                subscription: (bytes, peerId, serializer, roomManager, builder, udpMode) =>
                {
                    var @event = serializer.DeserializeContractLess<TEvent>(new ArraySegment<byte>(bytes));
                    handler(peerId, @event);
                },
                hookId: (byte)packetType);
        }

        public static void On<TEvent>(
            this IHost host,
            Action<Guid, TEvent, IRoomManager> handler,
            byte hookId)
        {
#pragma warning disable
            if (host == null) throw new ArgumentNullException(nameof(host));
#pragma warning restore

            host.OnCore<TEvent>(
                subscription: (bytes, peerId, serializer, roomManager, builder, udpMode) =>
                {
                    var @event = serializer.Deserialize<TEvent>(new ArraySegment<byte>(bytes));
                    handler(peerId,  @event, roomManager);
                },
                hookId: hookId);
        }

        public static void On<TEvent, TResponse>(
            this IHost host,
            Func<Guid, TEvent, IDatagramBuilder, Datagram<TResponse>> handler,
            byte hookId)
        {
#pragma warning disable
            if (host == null) throw new ArgumentNullException(nameof(host));
#pragma warning restore

            host.OnCore<TEvent>(
                subscription: (bytes, peerId, serializer, roomManager, builder, udpMode) =>
                {
                    var @event = serializer.Deserialize<TEvent>(new ArraySegment<byte>(bytes));
                    var dataGram = handler(peerId, @event, builder);

                    host.PublishInternal(datagram: dataGram, udpMode: udpMode, serializer.Serialize);
                },
                hookId);
        }

        public static void On<TEvent, TResponse>(
            this IHost host,
            Func<Guid, TEvent, IRoomManager, IDatagramBuilder, Datagram<TResponse>> handler,
            byte hookId)
        {
#pragma warning disable
            if (host == null) throw new ArgumentNullException(nameof(host));
#pragma warning restore

            host.OnCore<TEvent>(
                subscription: (bytes, peerId, serializer, roomManager, builder, udpMode) =>
                {
                    var @event = serializer.Deserialize<TEvent>(new ArraySegment<byte>(bytes));
                    var dataGram = handler(peerId, @event, roomManager, builder);

                    host.PublishInternal(datagram: dataGram, udpMode: udpMode, serializer.Serialize);
                },
                hookId);
        }
    }
}
