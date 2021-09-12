namespace UdpToolkit.Framework.Contracts
{
    using System;
    using UdpToolkit.Network.Contracts.Sockets;

    /// <summary>
    /// Extensions for creating subscriptions on events.
    /// </summary>
    public static class HostExtensions
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
            Action<Guid, IpV4Address, TEvent> onEvent,
            Action<Guid> onTimeout = null)
        {
#pragma warning disable
            if (host == null) throw new ArgumentNullException(nameof(host));
#pragma warning restore

            host.On(new Subscription<TEvent>(
                onEvent: onEvent,
                onTimeout: onTimeout));
        }
    }
}