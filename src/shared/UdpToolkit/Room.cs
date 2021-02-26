namespace UdpToolkit
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using UdpToolkit.Core;
    using UdpToolkit.Network;

    public sealed class Room : IRoom, IRawRoom
    {
        private readonly HashSet<Guid> _roomPeers = new HashSet<Guid>();

        public void AddPeer(Guid peerId)
        {
            _roomPeers.Add(peerId);
        }

        public void RemovePeer(Guid peerId)
        {
            _roomPeers.Remove(peerId);
        }

        public IEnumerable<Guid> GetPeers()
        {
            return _roomPeers.AsEnumerable();
        }

        public async Task Apply(
            Func<Guid, bool> condition,
            Func<Guid, Task> func)
        {
            for (var i = 0; i < _roomPeers.Count; i++)
            {
                var peerId = _roomPeers.ElementAt(i);
                if (!condition(peerId))
                {
                    continue;
                }

                await func(peerId).ConfigureAwait(false);
            }
        }
    }
}