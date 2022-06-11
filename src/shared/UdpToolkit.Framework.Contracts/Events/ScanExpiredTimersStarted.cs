namespace UdpToolkit.Framework.Contracts.Events
{
    /// <summary>
    /// Raised when scan expired timers started.
    /// </summary>
    public readonly struct ScanExpiredTimersStarted
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ScanExpiredTimersStarted"/> struct.
        /// </summary>
        /// <param name="timersCount">Times count.</param>
        public ScanExpiredTimersStarted(
            int timersCount)
        {
            TimersCount = timersCount;
        }

        /// <summary>
        /// Gets timers count.
        /// </summary>
        public int TimersCount { get; }
    }
}