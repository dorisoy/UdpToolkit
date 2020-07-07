namespace UdpToolkit.Integration.Tests.Resources
{
    using MessagePack;

    [MessagePackObject]
    public class Ping
    {
        public Ping(string payload)
        {
            Payload = payload;
        }

        [Key(0)]
        public string Payload { get; }
    }
}