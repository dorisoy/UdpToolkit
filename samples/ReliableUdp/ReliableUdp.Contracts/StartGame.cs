namespace ReliableUdp.Contracts
{
    using System;
    using System.Collections.Generic;
    using ProtoBuf;
    using UdpToolkit.Annotations;
    using UdpToolkit.Framework;

    [UdpEvent]
    [ProtoContract]
    public sealed class StartGame : IDisposable
    {
        [Obsolete("Serialization only")]
        public StartGame()
        {
        }

        public StartGame(
            Guid groupId,
            Dictionary<Guid, Position> positions)
        {
            GroupId = groupId;
            Positions = positions;
        }

        [ProtoMember(1)]
        public Guid GroupId { get; private set;  }

        [ProtoMember(2)]
        public Dictionary<Guid, Position> Positions { get; private set; }

        public void Dispose()
        {
            Console.WriteLine($"{this.GetType().Name} returned to pool.");
            ObjectsPool<StartGame>.Return(this);
        }

        public StartGame SetUp(
            Guid groupId,
            Dictionary<Guid, Position> spawnPositions)
        {
            GroupId = groupId;
            Positions = spawnPositions;
            return this;
        }
    }
}