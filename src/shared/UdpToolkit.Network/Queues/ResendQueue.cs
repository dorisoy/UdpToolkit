namespace UdpToolkit.Network.Queues
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using UdpToolkit.Network.Packets;

    /// <inheritdoc />
    internal sealed class ResendQueue : IResendQueue
    {
        private readonly ConcurrentDictionary<Guid, Lazy<List<PendingPacket>>> _resendQueue;

        /// <summary>
        /// Initializes a new instance of the <see cref="ResendQueue"/> class.
        /// </summary>
        internal ResendQueue()
        {
            _resendQueue = new ConcurrentDictionary<Guid, Lazy<List<PendingPacket>>>();
        }

        /// <inheritdoc />
        public void Add(
            Guid connectionId,
            PendingPacket pendingPacket)
        {
            var lazyQueue = _resendQueue.AddOrUpdate(
                key: connectionId,
                addValueFactory: (key) =>
                {
                    var queue = new Lazy<List<PendingPacket>>();
                    queue.Value.Add(pendingPacket);
                    return queue;
                },
                updateValueFactory: (key, queue) =>
                {
                    queue.Value.Add(pendingPacket);
                    return queue;
                });

            _ = lazyQueue.Value;
        }

        /// <inheritdoc />
        public List<PendingPacket> Get(
            Guid connectionId)
        {
            var lazyQueue = _resendQueue.GetOrAdd(
                key: connectionId,
                valueFactory: (key) => new Lazy<List<PendingPacket>>());

            return lazyQueue.Value;
        }
    }
}