namespace UdpToolkit.Network.Queues
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using UdpToolkit.Network.Packets;

    public class ResendQueue
    {
        private readonly ConcurrentDictionary<Guid, Lazy<List<ResendPacket>>> _resendQueue;

        public ResendQueue()
        {
            _resendQueue = new ConcurrentDictionary<Guid, Lazy<List<ResendPacket>>>();
        }

        public void Add(
            Guid connectionId,
            ResendPacket resendPAcket)
        {
            var lazyQueue = _resendQueue.AddOrUpdate(
                key: connectionId,
                addValueFactory: (key) =>
                {
                    var queue = new Lazy<List<ResendPacket>>();
                    queue.Value.Add(resendPAcket);
                    return queue;
                },
                updateValueFactory: (key, queue) =>
                {
                    queue.Value.Add(resendPAcket);
                    return queue;
                });

            _ = lazyQueue.Value;
        }

        public List<ResendPacket> Get(
            Guid connectionId)
        {
            var lazyQueue = _resendQueue.GetOrAdd(
                key: connectionId,
                valueFactory: (key) => new Lazy<List<ResendPacket>>());

            return lazyQueue.Value;
        }
    }
}