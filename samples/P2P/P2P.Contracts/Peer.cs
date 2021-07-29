namespace P2P.Contracts
{
    using MessagePack;

    [MessagePackObject]
    public class Peer
    {
        public Peer(
            string address,
            ushort port)
        {
            Address = address;
            Port = port;
        }

        [Key(0)]
        public string Address { get; }

        [Key(1)]
        public ushort Port { get; }
    }
}