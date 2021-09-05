namespace UdpToolkit.Framework.Contracts
{
    using System;

    /// <summary>
    /// QueueDispatcher, abstraction for dispatch all data from connection each time into the same queue.
    /// </summary>
    /// <typeparam name="TItem">
    /// Type of item for en queue.
    /// </typeparam>
    public interface IQueueDispatcher<TItem> : IDisposable
    {
        /// <summary>
        /// Gets count of queues available for dispatch.
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Indexer for get queue by specific index O(1).
        /// </summary>
        /// <param name="index">Index of queue.</param>
        IAsyncQueue<TItem> this[int index] { get; set; }

        /// <summary>
        /// Dispatch connection data to queue.
        /// </summary>
        /// <param name="connectionId">ConnectionId.</param>
        /// <returns>Queue for connection.</returns>
        IAsyncQueue<TItem> Dispatch(
            Guid connectionId);
    }
}