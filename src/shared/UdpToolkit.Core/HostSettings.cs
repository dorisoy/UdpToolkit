namespace UdpToolkit.Core
{
    using System;
    using System.Collections.Generic;
    using UdpToolkit.Serialization;

    public class HostSettings
    {
        public IEnumerable<int> OutputPorts { get; set; } = Array.Empty<int>();

        public IEnumerable<int> InputPorts { get; set; } = Array.Empty<int>();

        public int Workers { get; set; }

        public ISerializer Serializer { get; set; }

        public TimeSpan ResendPacketsTimeout { get; set; } = TimeSpan.FromSeconds(5);

        public TimeSpan PeerInactivityTimeout { get; set; } = TimeSpan.FromSeconds(5);

        public string Host { get; set; }
    }
}