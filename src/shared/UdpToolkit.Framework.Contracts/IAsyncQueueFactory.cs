namespace UdpToolkit.Framework.Contracts
{
    using UdpToolkit.Framework.Contracts.Events;

    /// <summary>
    /// Abstraction for create async queue instances.
    /// </summary>
    public interface IAsyncQueueFactory
    {
        /// <summary>
        /// Create queue instance.
        /// </summary>
        /// <param name="id">Queue identifier.</param>
        /// <param name="hostEventReporter">Instance of hostEventReporter.</param>
        /// <returns>Queue instance.</returns>
        /// <typeparam name="TItem">Type of queue item.</typeparam>
        IAsyncQueue<TItem> Create<TItem>(string id, IHostEventReporter hostEventReporter);
    }
}