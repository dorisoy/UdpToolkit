namespace UdpToolkit.Framework.Contracts
{
    /// <summary>
    /// Abstraction for exposing internal services.
    /// </summary>
    public interface IServiceProvider
    {
        /// <summary>
        /// Gets group manager.
        /// </summary>
        IGroupManager GroupManager { get; }

        /// <summary>
        /// Gets broadcaster.
        /// </summary>
        IBroadcaster Broadcaster { get; }
    }
}