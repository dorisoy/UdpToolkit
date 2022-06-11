namespace UdpToolkit.Framework.Contracts.Events
{
    /// <summary>
    /// Raised when expired groups started.
    /// </summary>
    public readonly struct ScanExpiredGroupsStarted
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ScanExpiredGroupsStarted"/> struct.
        /// </summary>
        /// <param name="groupsCount">Groups count.</param>
        public ScanExpiredGroupsStarted(
            int groupsCount)
        {
            GroupsCount = groupsCount;
        }

        /// <summary>
        /// Gets groups count.
        /// </summary>
        public int GroupsCount { get; }
    }
}