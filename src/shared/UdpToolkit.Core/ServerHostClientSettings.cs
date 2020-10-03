namespace UdpToolkit.Core
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class ServerHostClientSettings
    {
        public TimeSpan ResendPacketsTimeout { get; set; } = TimeSpan.FromSeconds(5);

        public TimeSpan ConnectionTimeout { get; set; } = TimeSpan.FromSeconds(5);

        public string ServerHost { get; set; } = "0.0.0.0";

        public IEnumerable<int> ServerPorts { get; set; } = Enumerable.Empty<int>();
    }
}