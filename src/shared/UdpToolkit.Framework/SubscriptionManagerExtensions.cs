namespace UdpToolkit.Framework
{
    using System;
    using UdpToolkit.Core;

    public static class SubscriptionManagerExtensions
    {
        internal static void OnProtocolInternal<TEvent>(this ISubscriptionManager host, Action<Guid, TEvent> handler, byte hookId)
        {
#pragma warning disable
            if (host == null) throw new ArgumentNullException(nameof(host));
#pragma warning restore

            host.Subscribe<TEvent>(
                subscription: (bytes, peerId, serializer, builder,  udpMode) =>
                {
                    var @event = serializer.DeserializeContractLess<TEvent>(new ArraySegment<byte>(bytes));
                    handler(peerId, @event);
                },
                hookId: hookId);
        }
    }
}