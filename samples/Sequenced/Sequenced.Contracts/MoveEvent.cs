namespace Sequenced.Contracts
{
    using MessagePack;

    [MessagePackObject]
    public class MoveEvent
    {
        public MoveEvent(int id, int roomId)
        {
            Id = id;
            RoomId = roomId;
        }

        [Key(0)]
        public int RoomId { get; set; }

        [Key(1)]
        public int Id { get; set; }
    }
}