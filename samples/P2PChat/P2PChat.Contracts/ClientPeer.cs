namespace P2PChat.Contracts
{
    using System;
    using MessagePack;

    [MessagePackObject]
    public class ClientPeer
    {
        [Obsolete(message: "For deserialization only", error: true)]
        public ClientPeer()
        {
        }

        public ClientPeer(
            string ip,
            int port)
        {
            Ip = ip;
            Port = port;
        }

        [Key(0)]
        public string Ip { get; set; }

        [Key(1)]
        public int Port { get; set; }
    }
}