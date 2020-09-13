namespace UdpToolkit.Core.ProtocolEvents
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;

    public sealed class Connect
    {
        public Connect(
            string clientHost,
            List<int> clientInputPorts)
        {
            ClientHost = clientHost;
            ClientInputPorts = clientInputPorts;
        }

        public string ClientHost { get; }

        public List<int> ClientInputPorts { get; }

        public List<IPEndPoint> GetPeerIps()
        {
            return ClientInputPorts
                .Select((port) => new IPEndPoint(IPAddress.Parse(ClientHost), port))
                .ToList();
        }
    }
}