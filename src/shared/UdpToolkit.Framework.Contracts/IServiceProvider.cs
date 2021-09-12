namespace UdpToolkit.Framework.Contracts
{
    /// <summary>
    /// Abstraction for exposing internal services.
    /// </summary>
    public interface IServiceProvider
    {
        /// <summary>
        /// Gets room manager.
        /// </summary>
        IRoomManager RoomManager { get; }

        /// <summary>
        /// Gets scheduler.
        /// </summary>
        IScheduler Scheduler { get; }

        /// <summary>
        /// Gets broadcaster.
        /// </summary>
        IBroadcaster Broadcaster { get; }
    }
}