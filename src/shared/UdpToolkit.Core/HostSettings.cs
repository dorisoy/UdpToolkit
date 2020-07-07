namespace UdpToolkit.Core
{
    using System.Collections.Generic;
    using UdpToolkit.Serialization;

    public class HostSettings
    {
        public IEnumerable<int> OutputPorts { get; set; }

        public IEnumerable<int> InputPorts { get; set; }

        public int Senders { get; set; }

        public int Receivers { get; set; }

        public int Workers { get; set; }

        public ISerializer Serializer { get; set; }

        public string Host { get; set; }
    }
}