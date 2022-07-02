namespace UdpToolkit.Framework
{
    using UdpToolkit.Framework.Contracts;

    /// <inheritdoc />
    public sealed class ServiceProvider : Contracts.IServiceProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceProvider"/> class.
        /// </summary>
        /// <param name="groupManager">Instance of group manager.</param>
        /// <param name="broadcaster">Instance of broadcaster.</param>
        public ServiceProvider(
            IGroupManager groupManager,
            IBroadcaster broadcaster)
        {
            GroupManager = groupManager;
            Broadcaster = broadcaster;
        }

        /// <inheritdoc />
        public IGroupManager GroupManager { get; }

        /// <inheritdoc />
        public IBroadcaster Broadcaster { get; }
    }
}