namespace ReliableUdp.Contracts
{
    using System;
    using System.Collections.Generic;
    using UdpToolkit.Annotations;

    [UdpEvent]
    public class StartGame
    {
        public StartGame(
            Guid groupId,
            Dictionary<Guid, Position> positions)
        {
            GroupId = groupId;
            Positions = positions;
        }

        public Guid GroupId { get; }

        public Dictionary<Guid, Position> Positions { get; }
    }
}