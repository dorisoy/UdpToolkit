namespace UdpToolkit.Framework.Contracts
{
    using System;
    using System.Collections.Generic;
    using UdpToolkit.Network.Contracts.Connections;

    /// <summary>
    /// Represent logical scope of connections.
    /// </summary>
    public sealed class Group
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Group"/> class.
        /// </summary>
        /// <param name="id">Group identifier.</param>
        /// <param name="groupConnections">List of group connections.</param>
        /// <param name="createdAt">Date of creation.</param>
        public Group(
            Guid id,
            List<IConnection> groupConnections,
            DateTimeOffset createdAt)
        {
            GroupConnections = groupConnections;
            CreatedAt = createdAt;
            Id = id;
        }

        /// <summary>
        /// Gets group identifier.
        /// </summary>
        public Guid Id { get; }

        /// <summary>
        /// Gets list of group connections.
        /// </summary>
        public List<IConnection> GroupConnections { get; }

        /// <summary>
        /// Gets creation date.
        /// </summary>
        public DateTimeOffset CreatedAt { get; }
    }
}