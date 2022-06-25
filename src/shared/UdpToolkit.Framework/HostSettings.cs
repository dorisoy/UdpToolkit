namespace UdpToolkit.Framework
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UdpToolkit.Framework.Contracts;
    using UdpToolkit.Framework.Contracts.Events;
    using UdpToolkit.Serialization;

    /// <summary>
    /// Host settings.
    /// </summary>
    public class HostSettings
    {
        private int? _workers;

        /// <summary>
        /// Initializes a new instance of the <see cref="HostSettings"/> class.
        /// </summary>
        /// <param name="serializer">Instance of serializer.</param>
        public HostSettings(
            ISerializer serializer)
        {
            Serializer = serializer;
        }

        /// <summary>
        /// Gets or sets host ip address in string representation.
        /// </summary>
        public string Host { get; set; } = "127.0.0.1";

        /// <summary>
        /// Gets or sets set of ports for the host.
        /// </summary>
        /// <example>
        /// Host = "127.0.0.1"
        /// HostPorts = [ 3000, 3001 ]
        /// Will be generated two host ips:
        /// 127.0.0.1:3000
        /// 127.0.0.1:3001.
        /// on each ip address would be two independent thread:
        /// 1) receiver thread
        /// 2) sender thread.
        /// </example>
        public IEnumerable<int> HostPorts { get; set; } = Array.Empty<int>();

        /// <summary>
        /// Gets or sets the count of workers for process user-defined subscriptions.
        /// </summary>
        /// <remarks>
        /// 1 Thread per worker.
        /// </remarks>
        public int Workers
        {
            get
            {
                return _workers ?? Environment.ProcessorCount - (HostPorts.Count() * 2);
            }

            set
            {
                _workers = value;
            }
        }

        /// <summary>
        /// Gets instance of serializer.
        /// </summary>
        public ISerializer Serializer { get; }

        /// <summary>
        /// Gets or sets instance of host event reporter.
        /// </summary>
        public IHostEventReporter HostEventReporter { get; set; } = new DefaultHostEventReporter();

        /// <summary>
        /// Gets or sets frequency of cleanup expired timers.
        /// </summary>
        public TimeSpan TimersCleanupFrequency { get; set; } = TimeSpan.FromSeconds(10);

        /// <summary>
        /// Gets or sets frequency of cleanup inactive groups.
        /// </summary>
        public TimeSpan GroupsCleanupFrequency { get; set; } = TimeSpan.FromSeconds(10);

        /// <summary>
        /// Gets or sets ttl for each group.
        /// </summary>
        /// <remarks>
        /// Typically equal to the time of game session.
        /// </remarks>
        public TimeSpan GroupTtl { get; set; } = TimeSpan.FromMinutes(10);

        /// <summary>
        /// Gets or sets Executor instance.
        /// </summary>
        public IExecutor Executor { get; set; } = new ThreadBasedExecutor();

        /// <summary>
        /// Gets or sets resends packets interval.
        /// </summary>
        public TimeSpan ResendPacketsInterval { get; set; } = TimeSpan.FromSeconds(1);
    }
}