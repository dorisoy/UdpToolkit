namespace UdpToolkit.Framework
{
    using System;
    using UdpToolkit.Core;

    public static class HostExtensions
    {
        public static void On<TEvent>(this IHost host, Action<Guid, TEvent> handler, byte hookId)
        {
#pragma warning disable
            if (host == null) throw new ArgumentNullException(nameof(host));
#pragma warning restore

            host.OnCore<TEvent>(
                subscription: (bytes, peerId, serializer, builder,  udpMode) =>
                {
                    var @event = serializer.Deserialize<TEvent>(new ArraySegment<byte>(bytes));
                    handler(peerId, @event);
                },
                hookId: hookId);
        }

        public static void On<TEvent, TResponse>(this IHost host, Func<Guid, TEvent, IDataGramBuilder, DataGram<TResponse>> handler, byte hookId)
        {
#pragma warning disable
            if (host == null) throw new ArgumentNullException(nameof(host));
#pragma warning restore

            host.OnCore<TEvent>(
                subscription: (bytes, peerId, serializer, builder,  udpMode) =>
                {
                    var @event = serializer.Deserialize<TEvent>(new ArraySegment<byte>(bytes));
                    var dataGram = handler(peerId, @event, builder);

                    host.PublishCore(dataGram: dataGram, udpMode: udpMode, serializer.Serialize);
                },
                hookId);
        }

        internal static void OnProtocolInternal<TEvent, TResponse>(this IHost host, Func<Guid, TEvent, IDataGramBuilder, DataGram<TResponse>> handler, byte hookId)
        {
#pragma warning disable
            if (host == null) throw new ArgumentNullException(nameof(host));
#pragma warning restore

            host.OnCore<TEvent>(
                subscription: (bytes, peerId, serializer, builder,  udpMode) =>
                {
                    var @event = serializer.DeserializeContractLess<TEvent>(new ArraySegment<byte>(bytes));
                    var dataGram = handler(peerId, @event, builder);

                    host.PublishCore(dataGram: dataGram, udpMode: udpMode, serializer.SerializeContractLess);
                },
                hookId);
        }
    }
}
