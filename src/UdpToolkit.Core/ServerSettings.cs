namespace UdpToolkit.Core
{
    using System.Collections.Generic;

    public sealed class ServerSettings
    {
        public int InputQueueBoundedCapacity { get; set; } = int.MaxValue;

        public int OutputQueueBoundedCapacity { get; set; } = int.MaxValue;

        public int ProcessWorkers { get; set; }

        public IEnumerable<int> InputPorts { get; set; }

        public IEnumerable<int> OutputPorts { get; set; }

        public ISerializer Serializer { get; set; }

        public string ServerHost { get; set; }
    }
}