namespace UdpToolkit.Framework.Contracts.Events
{
    /// <summary>
    /// Raised when queue item consumed.
    /// </summary>
    public readonly struct QueueItemConsumed
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QueueItemConsumed"/> struct.
        /// </summary>
        /// <param name="id">Queue identifier.</param>
        public QueueItemConsumed(
            string id)
        {
            Id = id;
        }

        /// <summary>
        /// Gets queue identifier.
        /// </summary>
        public string Id { get; }
    }
}