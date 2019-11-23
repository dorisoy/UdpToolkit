using MessagePack;

namespace SimpleUdp.Contracts
{
    [MessagePackObject()]
    public class AddResponse
    {
        [Key(0)]
        public int Sum { get; set; }
    }
}