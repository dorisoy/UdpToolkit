namespace UdpToolkit.Core
{
    using System;
    using System.Collections.Generic;

    public class HostClientSettings
    {
        public TimeSpan ResendPacketsTimeout { get; set; } = TimeSpan.FromSeconds(15);

        public TimeSpan ConnectionTimeout { get; set; } = TimeSpan.FromSeconds(15);

        public string ServerHost { get; set; } = "127.0.0.1";

        public IEnumerable<int> ServerInputPorts { get; set; } = Array.Empty<int>();

        public int? PingDelayInMs { get; set; } = 2000;
    }
}