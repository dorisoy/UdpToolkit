namespace UdpToolkit.Framework.Contracts.Events
{
    /// <summary>
    /// Raised when expired timer removed.
    /// </summary>
    public readonly struct ExpiredTimerRemoved
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExpiredTimerRemoved"/> struct.
        /// </summary>
        /// <param name="timerKey">Timer key.</param>
        public ExpiredTimerRemoved(
            TimerKey timerKey)
        {
            TimerKey = timerKey;
        }

        /// <summary>
        /// Gets timer key.
        /// </summary>
        public TimerKey TimerKey { get; }
    }
}