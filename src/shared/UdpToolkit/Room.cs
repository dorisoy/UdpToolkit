namespace UdpToolkit
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using UdpToolkit.Core;

    public sealed class Room : IRoom, IRawRoom
    {
        private readonly ConcurrentDictionary<Guid, Peer> _roomPeers = new ConcurrentDictionary<Guid, Peer>();

        public void AddPeer(IPeer peer)
        {
            _roomPeers[peer.PeerId] = peer as Peer;
        }

        public void RemovePeer(Guid peerId)
        {
            _roomPeers.TryRemove(peerId, out _);
        }

        public IEnumerable<Guid> GetPeers()
        {
            return _roomPeers.Keys;
        }

        public async Task Apply(
            Func<Peer, bool> condition,
            Func<Peer, Task> func)
        {
            for (var i = 0; i < _roomPeers.Count; i++)
            {
                var pair = _roomPeers.ElementAt(i);
                var peer = pair.Value;
                if (!condition(peer))
                {
                    continue;
                }

                await func(pair.Value).ConfigureAwait(false);
            }
        }
    }
}