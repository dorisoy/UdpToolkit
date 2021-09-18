namespace UdpToolkit.Framework.Contracts
{
    using System;

    /// <summary>
    /// Key for scheduled action.
    /// </summary>
    public readonly struct TimerKey
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TimerKey"/> struct.
        /// </summary>
        /// <param name="timerId">Timer identifier.</param>
        /// <param name="eventType">Event type.</param>
        public TimerKey(
            Guid timerId,
            Type eventType)
        {
            TimerId = timerId;
            EventType = eventType;
        }

        /// <summary>
        /// Gets timer identifier.
        /// </summary>
        public Guid TimerId { get; }

        /// <summary>
        /// Gets timer identifier.
        /// </summary>
        public Type EventType { get; }

        /// <summary>
        /// Equals.
        /// </summary>
        /// <param name="other">Other key.</param>
        /// <returns>True/False.</returns>
        public bool Equals(TimerKey other)
        {
            return TimerId.Equals(other.TimerId) && Equals(EventType, other.EventType);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return obj is TimerKey other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                return (TimerId.GetHashCode() * 397) ^ (EventType != null ? EventType.GetHashCode() : 0);
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{TimerId}|{EventType.Name}";
        }
    }
}