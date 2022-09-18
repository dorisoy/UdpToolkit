namespace Structs.Contracts
{
    using System;
    using UdpToolkit.Annotations;

    [UdpEvent]
    public readonly struct StartGame
    {
        public StartGame(SpawnPositions spawnPositions, Guid groupId)
        {
            SpawnPositions = spawnPositions;
            GroupId = groupId;
        }

        public Guid GroupId { get; }

        public SpawnPositions SpawnPositions { get; }
    }

    public unsafe struct SpawnPositions
    {
#pragma warning disable
        public fixed float X[Consts.RoomSize];
        public fixed float Y[Consts.RoomSize];
        public fixed float Z[Consts.RoomSize];
    }
}