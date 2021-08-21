namespace Sequenced.Contracts
{
    using MessagePack;
    using UdpToolkit.Annotations;

    [UdpEvent]
    [MessagePackObject]
    public class MoveEvent
    {
        public MoveEvent(
            int roomId,
            int id,
            string @from)
        {
            Id = id;
            RoomId = roomId;
            From = @from;
        }

        [Key(0)]
        public int RoomId { get; }

        [Key(1)]
        public int Id { get; }

        [Key(2)]
        public string From { get; }
    }
}