namespace UdpToolkit.Framework.Contracts.Events
{
    /// <summary>
    /// Raised when host started.
    /// </summary>
    public readonly struct HostStarted
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HostStarted"/> struct.
        /// </summary>
        /// <param name="receiversCount">Receivers count.</param>
        /// <param name="sendersCount">Senders count.</param>
        /// <param name="workersCount">Workers count.</param>
        /// <param name="executor">Instance of executor.</param>
        public HostStarted(
            int receiversCount,
            int sendersCount,
            int workersCount,
            IReadOnlyExecutor executor)
        {
            ReceiversCount = receiversCount;
            SendersCount = sendersCount;
            WorkersCount = workersCount;
            Executor = executor;
        }

        /// <summary>
        /// Gets receivers count.
        /// </summary>
        public int ReceiversCount { get; }

        /// <summary>
        /// Gets senders count.
        /// </summary>
        public int SendersCount { get; }

        /// <summary>
        /// Gets workers count.
        /// </summary>
        public int WorkersCount { get; }

        /// <summary>
        /// Gets host executor.
        /// </summary>
        public IReadOnlyExecutor Executor { get; }
    }
}