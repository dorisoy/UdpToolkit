// ReSharper disable once CheckNamespace
namespace UdpToolkit.Framework.Contracts
{
    using System;
    using UdpToolkit.Network.Contracts.Clients;
    using UdpToolkit.Serialization;

    /// <summary>
    /// Abstraction for processing input and output host packets.
    /// </summary>
    public interface IHostWorker : IDisposable
    {
        /// <summary>
        /// Gets or sets instance of serializer.
        /// </summary>
        ISerializer Serializer { get; set; }

        /// <summary>
        /// Gets or sets instance of broadcaster.
        /// </summary>
        IBroadcaster Broadcaster { get; set; }

        /// <summary>
        /// Process input packet.
        /// </summary>
        /// <param name="inPacket">Input packet.</param>
        void Process(
            InNetworkPacket inPacket);

        /// <summary>
        /// Process output packet.
        /// </summary>
        /// <param name="outPacket">Output packet.</param>
        void Process(
            OutNetworkPacket outPacket);

        /// <summary>
        /// Process output packet.
        /// </summary>
        /// <param name="type">Type of event.</param>
        /// <param name="subscriptionId">Subscription identifier.</param>
        /// <returns>True if subscription exists.</returns>
        bool TryGetSubscriptionId(
            Type type,
            out byte subscriptionId);
    }
}