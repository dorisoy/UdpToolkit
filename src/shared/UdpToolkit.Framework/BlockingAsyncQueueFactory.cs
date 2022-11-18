namespace UdpToolkit.Framework
{
    using UdpToolkit.Framework.Contracts;
    using UdpToolkit.Framework.Contracts.Events;

    /// <inheritdoc />
    public sealed class BlockingAsyncQueueFactory : IAsyncQueueFactory
    {
        /// <inheritdoc />
        public IAsyncQueue<TItem> Create<TItem>(
            string id,
            IHostEventReporter hostEventReporter)
        {
            return new BlockingAsyncQueue<TItem>(
                id: id,
                hostEventReporter: hostEventReporter);
        }
    }
}