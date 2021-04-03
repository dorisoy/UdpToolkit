namespace UdpToolkit.Core
{
    using System;
    using System.Collections.Generic;
    using UdpToolkit.Core.Executors;
    using UdpToolkit.Logging;
    using UdpToolkit.Serialization;

    public class HostSettings
    {
        public IEnumerable<int> OutputPorts { get; set; } = Array.Empty<int>();

        public IEnumerable<int> InputPorts { get; set; } = Array.Empty<int>();

        public int Workers { get; set; } = 2;

        public ISerializer Serializer { get; set; }

        public IUdpToolkitLoggerFactory LoggerFactory { get; set; }

        public TimeSpan RoomsCleanupFrequency { get; set; } = TimeSpan.FromSeconds(10);

        public TimeSpan RoomTtl { get; set; } = TimeSpan.FromMinutes(2);

        public TimeSpan ConnectionTtl { get; set; } = TimeSpan.FromSeconds(30);

        public TimeSpan ConnectionsCleanupFrequency { get; set; } = TimeSpan.FromSeconds(10);

        public TimeSpan ResendPacketsTimeout { get; set; } = TimeSpan.FromSeconds(30);

        public ExecutorType ExecutorType { get; set; } = ExecutorType.ThreadBasedExecutor;

        public string Host { get; set; } = "127.0.0.1";
    }
}