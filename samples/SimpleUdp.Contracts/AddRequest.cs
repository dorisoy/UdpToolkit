using MessagePack;

namespace SimpleUdp.Contracts
{
    [MessagePackObject]
    public class AddRequest
    {
        [Key(0)]
        public int X { get; set; }
        
        [Key(1)]
        public int Y { get; set; }
    }
}