namespace UdpToolkit.Framework.Contracts.Settings
{
    using System;
    using System.Collections.Generic;
    using UdpToolkit.Framework.Contracts.Executors;
    using UdpToolkit.Logging;
    using UdpToolkit.Serialization;

    public class HostSettings
    {
        public HostSettings(
            ISerializer serializer,
            IUdpToolkitLoggerFactory loggerFactory)
        {
            Serializer = serializer;
            LoggerFactory = loggerFactory;
        }

        public string Host { get; set; } = "127.0.0.1";

        public IEnumerable<int> HostPorts { get; set; } = Array.Empty<int>();

        public int Workers { get; set; } = 8;

        public ISerializer Serializer { get; }

        public IUdpToolkitLoggerFactory LoggerFactory { get; }

        public TimeSpan RoomsCleanupFrequency { get; set; } = TimeSpan.FromSeconds(10);

        public TimeSpan RoomTtl { get; set; } = TimeSpan.FromMinutes(10);

        public IExecutor Executor { get; set; } = new ThreadBasedExecutor();
    }
}