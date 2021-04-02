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
            Guid connectionId)
        {
            _rooms.AddOrUpdate(
                key: roomId,
                addValueFactory: (id) => new List<Guid>
                {
                    connectionId,
                },
                updateValueFactory: (id, room) =>
                {
                    if (!room.Contains(connectionId))
                    {
                        room.Add(connectionId);
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
            Guid connectionId)
        {
            _rooms[roomId].Remove(connectionId);
        }
    }
}