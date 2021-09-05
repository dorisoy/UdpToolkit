namespace UdpToolkit.Framework.Contracts
{
    using System;
    using UdpToolkit.Network.Contracts.Sockets;

    /// <summary>
    /// Extensions for creating subscriptions on events, without broadcasting (Client semantic).
    /// </summary>
    public static class HostClientExtensions
    {
        /// <summary>
        /// Subscribing on the `event`.
        /// </summary>
        /// <param name="host">Instance of host.</param>
        /// <param name="onEvent">Callback for `event`.</param>
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
            Action<Guid> onTimeout = null)
        {
#pragma warning disable
            if (host == null) throw new ArgumentNullException(nameof(host));
#pragma warning restore

            host.On(onEvent, BroadcastMode.None, onTimeout);
        }

        /// <summary>
        /// Subscribing on the `event`.
        /// </summary>
        /// <param name="host">Instance of host.</param>
        /// <param name="onEvent">Callback for `event`.</param>
        /// <param name="onTimeout">Timeout callback.</param>
        /// <typeparam name="TInEvent">Type of user-defined input event.</typeparam>
        /// <exception cref="ArgumentNullException">If host instance is null.</exception>
        public static void On<TInEvent>(
            this IHost host,
            Func<Guid, IpV4Address, TInEvent, IRoomManager> onEvent,
            Action<Guid> onTimeout = null)
        {
#pragma warning disable
            if (host == null) throw new ArgumentNullException(nameof(host));
#pragma warning restore

            host.On(onEvent, onTimeout);
        }

        /// <summary>
        /// Subscribing on the `event`.
        /// </summary>
        /// <param name="host">Instance of host.</param>
        /// <param name="onEvent">Callback for `event`.</param>
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
            Action<Guid> onTimeout = null)
        {
#pragma warning disable
            if (host == null) throw new ArgumentNullException(nameof(host));
#pragma warning restore

            host.On(onEvent, BroadcastMode.None, onTimeout);
        }
    }
}