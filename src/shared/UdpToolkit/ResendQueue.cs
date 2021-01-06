namespace UdpToolkit
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using UdpToolkit.Network.Packets;
    using UdpToolkit.Network.Pooling;

    public sealed class ResendQueue
    {
        private readonly ConcurrentDictionary<Guid, Lazy<List<PooledObject<NetworkPacket>>>> _resendQueue;

        public ResendQueue()
        {
            _resendQueue = new ConcurrentDictionary<Guid, Lazy<List<PooledObject<NetworkPacket>>>>();
        }

        public void Add(
            PooledObject<NetworkPacket> pooledNetworkPacket)
        {
            var lazyQueue = _resendQueue.AddOrUpdate(
                key: pooledNetworkPacket.Value.PeerId,
                addValueFactory: (key) =>
                {
                    var queue = new Lazy<List<PooledObject<NetworkPacket>>>();
                    queue.Value.Add(pooledNetworkPacket);
                    return queue;
                },
                updateValueFactory: (key, queue) =>
                {
                    queue.Value.Add(pooledNetworkPacket);
                    return queue;
                });

            _ = lazyQueue.Value;
        }

        public List<PooledObject<NetworkPacket>> Get(
            Guid peerId)
        {
            var lazyQueue = _resendQueue.GetOrAdd(
                key: peerId,
                valueFactory: (key) => new Lazy<List<PooledObject<NetworkPacket>>>());

            return lazyQueue.Value;
        }
    }
}