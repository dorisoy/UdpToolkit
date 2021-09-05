namespace UdpToolkit.Framework.Contracts
{
    /// <summary>
    /// Data structure for represent events outgoing of the host.
    /// </summary>
    /// <typeparam name="TEvent">
    /// Type of user-defined event.
    /// </typeparam>
    public readonly struct OutEvent<TEvent>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OutEvent{TEvent}"/> struct.
        /// </summary>
        /// <param name="roomId">Room identifier.</param>
        /// <param name="event">Instance of user-defined event.</param>
        /// <param name="delayInMs">Delay for sending in ms.</param>
        /// <param name="broadcastMode">Broadcast mode.</param>
        /// <param name="channelId">Channel identifier.</param>
        public OutEvent(
            int roomId,
            TEvent @event,
            int delayInMs,
            BroadcastMode broadcastMode,
            byte channelId)
        {
            RoomId = roomId;
            Event = @event;
            DelayInMs = delayInMs;
            BroadcastMode = broadcastMode;
            ChannelId = channelId;
        }

        /// <summary>
        /// Gets room identifier.
        /// </summary>
        public int RoomId { get; }

        /// <summary>
        /// Gets instance of user-defined event.
        /// </summary>
        public TEvent Event { get; }

        /// <summary>
        /// Gets value of delay for sending in ms.
        /// </summary>
        public int DelayInMs { get; }

        /// <summary>
        /// Gets value of broadcast mode.
        /// </summary>
        public BroadcastMode BroadcastMode { get; }

        /// <summary>
        /// Gets channel identifier.
        /// </summary>
        public byte ChannelId { get; }
    }
}