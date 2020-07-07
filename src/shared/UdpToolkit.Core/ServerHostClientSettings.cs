namespace UdpToolkit.Core
{
    using System.Collections.Generic;
    using System.Linq;

    public class ServerHostClientSettings
    {
        public string ServerHost { get; set; } = "0.0.0.0";

        public IEnumerable<int> ServerPorts { get; set; } = Enumerable.Empty<int>();
    }
}