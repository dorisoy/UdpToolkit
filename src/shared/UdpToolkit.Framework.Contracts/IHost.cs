namespace UdpToolkit.Framework.Contracts
{
    using System;

    /// <summary>
    /// Host, keep together sending, receiving, and processing logic.
    /// </summary>
    public interface IHost : IDisposable
    {
        /// <summary>
        /// Gets client, for interacting with other hosts.
        /// </summary>
        IHostClient HostClient { get; }

        /// <summary>
        /// Run host.
        /// </summary>
        void Run();

        /// <summary>
        /// Subscribing on user-defined event.
        /// </summary>
        /// <param name="subscription">Subscription instance.</param>
        /// <typeparam name="TEvent">User-defined event.</typeparam>
        void On<TEvent>(
            Subscription<TEvent> subscription);
    }
}
