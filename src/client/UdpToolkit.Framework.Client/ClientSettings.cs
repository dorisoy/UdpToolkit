namespace UdpToolkit.Framework.Client
{
    using System.Collections.Generic;
    using UdpToolkit.Core;

    public class ClientSettings
    {
        public IEnumerable<int> ServerOutputPorts { get; set; }

        public IEnumerable<int> ServerInputPorts { get; set; }

        public int Senders { get; set; }

        public int Receivers { get; set; }

        public ISerializer Serializer { get; set; }

        public string ServerHost { get; set; }
    }
}