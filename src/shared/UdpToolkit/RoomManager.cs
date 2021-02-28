namespace UdpToolkit
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using UdpToolkit.Core;
    using UdpToolkit.Network;

    public sealed class RoomManager : IRoomManager
    {
        private readonly ConcurrentDictionary<int, List<Guid>> _rooms = new ConcurrentDictionary<int, List<Guid>>();

        public void JoinOrCreate(
            int roomId,
            Guid peerId)
        {
            _rooms.AddOrUpdate(
                key: roomId,
                addValueFactory: (id) => new List<Guid>
                {
                    peerId,
                },
                updateValueFactory: (id, room) =>
                {
                    if (!room.Contains(peerId))
                    {
                        room.Add(peerId);
                    }

                    return room;
                });
        }

        public List<Guid> GetRoom(int roomId)
        {
            return _rooms[roomId];
        }

        public void Leave(
            int roomId,
            Guid peerId)
        {
            _rooms[roomId].Remove(peerId);
        }
    }
}