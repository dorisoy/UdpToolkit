namespace UdpToolkit.Framework
{
    using System;
    using UdpToolkit.Framework.Contracts;
    using UdpToolkit.Network.Contracts.Sockets;

    public static class HostClientExtensions
    {
        public static void On<TEvent>(
            this IHost host,
            Func<Guid, IpV4Address, TEvent, int> onEvent,
            Action<Guid> onTimeout = null)
        {
#pragma warning disable
            if (host == null) throw new ArgumentNullException(nameof(host));
#pragma warning restore

            host.On(onEvent, BroadcastMode.None, onTimeout);
        }

        public static void On<TInEvent, TOutEvent>(
            this IHost host,
            Func<Guid, IpV4Address, TInEvent, IRoomManager, OutEvent<TOutEvent>> onEvent,
            Action<Guid> onTimeout = null)
        {
#pragma warning disable
            if (host == null) throw new ArgumentNullException(nameof(host));
#pragma warning restore

            host.On(onEvent, BroadcastMode.None, onTimeout);
        }

        public static void On<TEvent>(
            this IHost host,
            Func<Guid, IpV4Address, TEvent, IRoomManager, int> onEvent,
            Action<Guid> onTimeout = null)
        {
#pragma warning disable
            if (host == null) throw new ArgumentNullException(nameof(host));
#pragma warning restore

            host.On(onEvent, BroadcastMode.None, onTimeout);
        }
    }
}