namespace UdpToolkit.Core
{
    public class ClientIp
    {
        public ClientIp(
            string host,
            int port)
        {
            Host = host;
            Port = port;
        }

        public string Host { get; }

        public int Port { get; }
    }
}