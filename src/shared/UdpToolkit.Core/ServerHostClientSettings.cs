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

        public string ClientHost { get; set; } = "0.0.0.0";

        public IEnumerable<int> ServerInputPorts { get; set; } = Array.Empty<int>();

        public int? PingDelayInMs { get; set; } = 2000;
    }
}