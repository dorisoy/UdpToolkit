namespace UdpToolkit.Core
{
    using System;
    using System.Collections.Generic;
    using UdpToolkit.Logging;
    using UdpToolkit.Serialization;

    public class HostSettings
    {
        public IEnumerable<int> OutputPorts { get; set; } = Array.Empty<int>();

        public IEnumerable<int> InputPorts { get; set; } = Array.Empty<int>();

        public int Workers { get; set; } = 2;

        public ISerializer Serializer { get; set; }

        public IUdpToolkitLoggerFactory LoggerFactory { get; set; }

        public TimeSpan ResendPacketsTimeout { get; set; } = TimeSpan.FromSeconds(15);

        public TimeSpan InactivityTimeout { get; set; } = TimeSpan.FromSeconds(15);

        public string Host { get; set; } = "127.0.0.1";
    }
}