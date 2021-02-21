namespace ReliableUdp.Contracts
{
    using System;
    using MessagePack;

    [MessagePackObject]
    public class StartGame
    {
        public StartGame(
            int roomId,
            Guid peerId)
        {
            RoomId = roomId;
            PeerId = peerId;
        }

        [Key(0)]
        public int RoomId { get; set; }

        [Key(1)]
        public Guid PeerId { get; set; }
    }
}