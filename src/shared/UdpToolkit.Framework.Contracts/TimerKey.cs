namespace UdpToolkit.Framework.Contracts
{
    /// <summary>
    /// Key for scheduled action.
    /// </summary>
    public readonly struct TimerKey
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TimerKey"/> struct.
        /// </summary>
        /// <param name="roomId">Room identifier.</param>
        /// <param name="timerId">Timer identifier.</param>
        public TimerKey(
            int roomId,
            string timerId)
        {
            RoomId = roomId;
            TimerId = timerId;
        }

        /// <summary>
        /// Gets room identifier.
        /// </summary>
        public int RoomId { get; }

        /// <summary>
        /// Gets timer identifier.
        /// </summary>
        public string TimerId { get; }
    }
}