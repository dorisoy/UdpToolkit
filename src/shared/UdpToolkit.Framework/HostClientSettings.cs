namespace UdpToolkit.Framework
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Host client settings.
    /// </summary>
    public class HostClientSettings
    {
        /// <summary>
        /// Gets or sets timeout for connection.
        /// </summary>
        public TimeSpan ConnectionTimeout { get; set; } = TimeSpan.FromSeconds(15);

        /// <summary>
        /// Gets or sets remote host ip address in string representation.
        /// </summary>
        public string ServerHost { get; set; } = "127.0.0.1";

        /// <summary>
        /// Gets or sets remote host ports for receive packets.
        /// </summary>
        public IEnumerable<int> ServerPorts { get; set; } = Array.Empty<int>();

        /// <summary>
        /// Gets or sets delay for resend request.
        /// </summary>
        public int? ResendPacketsDelay { get; set; } = 1000;
    }
}