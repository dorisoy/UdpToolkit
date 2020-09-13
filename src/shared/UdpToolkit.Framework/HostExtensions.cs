namespace UdpToolkit.Framework
{
    using System;
    using System.Threading.Tasks;
    using Serilog;
    using UdpToolkit.Core;
    using UdpToolkit.Core.ProtocolEvents;
    using UdpToolkit.Network.Channels;

    public static class HostExtensions
    {
        public static Task RunHostAsync(this IHost host)
        {
#pragma warning disable
            if (host == null) throw new ArgumentNullException(nameof(host));
#pragma warning restore

            host.OnProtocolInternal<Connect, Connected>(
                handler: (peerId, connect, builder) =>
                {
                    Log.Logger.Information($"Peer with id - {peerId} connected");

                    return builder.Caller(new Connected(peerId), peerId, (byte)PacketType.Connected);
                },
                hookId: (byte)PacketType.Connect);

            host.OnProtocolInternal<Disconnect, Disconnected>(
                handler: (peerId, disconnect, builder) =>
                {
                    Log.Logger.Information($"Peer with id - {peerId} disconnected");

                    return builder.Caller(new Disconnected(peerId), peerId, (byte)PacketType.Disconnect);
                },
                hookId: (byte)PacketType.Disconnect);

            return host.RunAsync();
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
                subscription: (bytes, peerId, serializer, builder, udpMode) =>
                {
                    var @event = serializer.Deserialize<TEvent>(new ArraySegment<byte>(bytes));
                    handler(peerId, @event);
                },
                hookId: hookId);
        }

        public static void On<TEvent, TResponse>(
            this IHost host,
            Func<Guid, TEvent, IDataGramBuilder, DataGram<TResponse>> handler,
            byte hookId)
        {
#pragma warning disable
            if (host == null) throw new ArgumentNullException(nameof(host));
#pragma warning restore

            host.OnCore<TEvent>(
                subscription: (bytes, peerId, serializer, builder, udpMode) =>
                {
                    var @event = serializer.Deserialize<TEvent>(new ArraySegment<byte>(bytes));
                    var dataGram = handler(peerId, @event, builder);

                    host.PublishCore(dataGram: dataGram, udpMode: udpMode, serializer.Serialize);
                },
                hookId);
        }

        internal static void OnProtocolInternal<TEvent, TResponse>(
            this IHost host,
            Func<Guid, TEvent, IDataGramBuilder, DataGram<TResponse>> handler,
            byte hookId)
        {
#pragma warning disable
            if (host == null) throw new ArgumentNullException(nameof(host));
#pragma warning restore

            host.OnCore<TEvent>(
                subscription: (bytes, peerId, serializer, builder, udpMode) =>
                {
                    var @event = serializer.DeserializeContractLess<TEvent>(new ArraySegment<byte>(bytes));
                    var dataGram = handler(peerId, @event, builder);

                    host.PublishCore(dataGram: dataGram, udpMode: udpMode, serializer.SerializeContractLess);
                },
                hookId);
        }

        internal static void OnProtocolInternal<TEvent>(
            this IHost host,
            Action<Guid, TEvent> handler,
            byte hookId)
        {
#pragma warning disable
            if (host == null) throw new ArgumentNullException(nameof(host));
#pragma warning restore

            host.OnCore<TEvent>(
                subscription: (bytes, peerId, serializer, builder, udpMode) =>
                {
                    var @event = serializer.DeserializeContractLess<TEvent>(new ArraySegment<byte>(bytes));
                    handler(peerId, @event);
                },
                hookId: hookId);
        }
    }
}
