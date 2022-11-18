namespace UdpToolkit.Framework.Contracts
{
    using System;

    /// <summary>
    /// Abstraction for perform async operation with queues.
    /// </summary>
    /// <typeparam name="TItem">
    /// Type of item in the async queue.
    /// </typeparam>
    public interface IAsyncQueue<TItem> : IDisposable
    {
        /// <summary>
        /// Action for processing consumed items.
        /// </summary>
        event Action<TItem> OnItemConsumed;

        /// <summary>
        /// Produces items to the async queue.
        /// </summary>
        /// <param name="item">Produced item.</param>
        void Produce(
            TItem item);

        /// <summary>
        /// Consumes items in the async queue.
        /// </summary>
        void Consume();
    }
}