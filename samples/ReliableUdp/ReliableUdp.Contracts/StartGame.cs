namespace ReliableUdp.Contracts
{
    using MessagePack;

    [MessagePackObject]
    public class StartGame
    {
        public StartGame(int roomId)
        {
            RoomId = roomId;
        }

        [Key(0)]
        public int RoomId { get; set; }
    }
}