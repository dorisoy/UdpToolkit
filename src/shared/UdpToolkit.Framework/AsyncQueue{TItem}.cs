namespace UdpToolkit.Framework
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading.Channels;
    using UdpToolkit.Framework.Contracts;
    using UdpToolkit.Framework.Contracts.Events;

    /// <summary>
    /// Async queue based on .NET BlockingCollection.
    /// </summary>
    /// <typeparam name="TItem">
    /// Type of item in the async queue.
    /// </typeparam>
    public sealed class AsyncQueue<TItem> : IAsyncQueue<TItem>
    {
        private readonly string _id;
        private readonly Action<TItem> _action;
        private readonly Channel<TItem> _input;
        private readonly IHostEventReporter _hostEventReporter;

        private bool _disposed = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncQueue{TItem}"/> class.
        /// </summary>
        /// <param name="action">Action for process consumed items.</param>
        /// <param name="hostEventReporter">Host event reporter.</param>
        /// <param name="id">Queue identifier.</param>
        public AsyncQueue(
            Action<TItem> action,
            IHostEventReporter hostEventReporter,
            string id)
        {
            _action = action;
            _hostEventReporter = hostEventReporter;
            _id = id;
            _input = Channel.CreateUnbounded<TItem>();
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="AsyncQueue{TItem}"/> class.
        /// </summary>
        [ExcludeFromCodeCoverage]
        ~AsyncQueue()
        {
            Dispose(false);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Produces items to the async queue.
        /// </summary>
        /// <param name="item">Produced item.</param>
        public async void Produce(
            TItem item)
        {
            try
            {
                await _input.Writer.WriteAsync(item).ConfigureAwait(false);
            }
            catch (ObjectDisposedException)
            {
                // ignore
            }
        }

        /// <summary>
        /// Consumes items in the async queue.
        /// </summary>
        public async void Consume()
        {
            try
            {
                while (await _input.Reader.WaitToReadAsync().ConfigureAwait(false))
                {
                    while (_input.Reader.TryRead(out var item))
                    {
                        var queueItemConsumed = new QueueItemConsumed(_id);
                        _hostEventReporter.Handle(in queueItemConsumed);
                        _action(item);
                    }
                }
            }
            catch (ObjectDisposedException)
            {
                // ignore
            }
            catch (Exception ex)
            {
                var exceptionThrown = new ExceptionThrown(ex);
                _hostEventReporter.Handle(in exceptionThrown);
            }
        }

        private void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _input.Writer.Complete();
            }

            _disposed = true;
        }
    }
}