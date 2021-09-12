// ReSharper disable once CheckNamespace
namespace UdpToolkit.Framework.Contracts
{
    using System;
    using UdpToolkit.Logging;
    using UdpToolkit.Serialization;

    /// <summary>
    /// Abstraction for processing input and output host packets.
    /// </summary>
    public interface IHostWorker : IDisposable
    {
        /// <summary>
        /// Gets or sets instance of logger.
        /// </summary>
        ILogger Logger { get; set; }

        /// <summary>
        /// Gets or sets instance of serializer.
        /// </summary>
        ISerializer Serializer { get; set; }

        /// <summary>
        /// Process input packet.
        /// </summary>
        /// <param name="inPacket">Input packet.</param>
        public void Process(
            InPacket inPacket);

        /// <summary>
        /// Process output packet.
        /// </summary>
        /// <param name="outPacket">Output packet.</param>
        /// <returns>
        /// An array of bytes for sending over the network.
        /// </returns>
        public byte[] Process(
            OutPacket outPacket);
    }
}