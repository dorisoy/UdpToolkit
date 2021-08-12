namespace UdpToolkit.Framework.Contracts.Settings
{
    using System;
    using System.Collections.Generic;
    using UdpToolkit.Framework.Contracts.Executors;
    using UdpToolkit.Logging;
    using UdpToolkit.Serialization;

    public class HostSettings
    {
        public string Host { get; set; } = "127.0.0.1";

        public IEnumerable<int> HostPorts { get; set; } = Array.Empty<int>();

        public int Workers { get; set; } = 8;

        public ISerializer Serializer { get; set; }

        public IUdpToolkitLoggerFactory LoggerFactory { get; set; }

        public IConnectionIdFactory ConnectionIdFactory { get; set; } = new ConnectionIdFactory();

        public TimeSpan RoomsCleanupFrequency { get; set; } = TimeSpan.FromSeconds(10);

        public TimeSpan RoomTtl { get; set; } = TimeSpan.FromMinutes(10);

        public IExecutor Executor { get; set; } = new ThreadBasedExecutor();
    }
}