namespace P2P.Contracts
{
    using System;
    using MessagePack;
    using UdpToolkit.Annotations;

    [UdpEvent]
    [MessagePackObject]
    public sealed class Peer : IDisposable
    {
        [Obsolete("Serialization only")]
        public Peer()
        {
        }

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

        public void Dispose()
        {
            // nothing to do
        }
    }
}