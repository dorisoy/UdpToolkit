namespace UdpToolkit.Framework
{
    using UdpToolkit.Framework.Contracts;

    /// <inheritdoc />
    public sealed class ServiceProvider : Contracts.IServiceProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceProvider"/> class.
        /// </summary>
        /// <param name="roomManager">Instance of room manager.</param>
        /// <param name="scheduler">Instance of scheduler.</param>
        /// <param name="broadcaster">Instance of broadcaster.</param>
        public ServiceProvider(
            IRoomManager roomManager,
            IScheduler scheduler,
            IBroadcaster broadcaster)
        {
            RoomManager = roomManager;
            Scheduler = scheduler;
            Broadcaster = broadcaster;
        }

        /// <inheritdoc />
        public IRoomManager RoomManager { get; }

        /// <inheritdoc />
        public IScheduler Scheduler { get; }

        /// <inheritdoc />
        public IBroadcaster Broadcaster { get; }
    }
}