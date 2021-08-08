namespace UdpToolkit.Network.Queues
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using UdpToolkit.Network.Contracts.Packets;

    public sealed class ResendQueue
    {
        private readonly ConcurrentDictionary<Guid, Lazy<List<PendingPacket>>> _resendQueue;

        public ResendQueue()
        {
            _resendQueue = new ConcurrentDictionary<Guid, Lazy<List<PendingPacket>>>();
        }

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