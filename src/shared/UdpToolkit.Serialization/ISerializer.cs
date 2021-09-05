namespace UdpToolkit.Serialization
{
    using System;

    /// <summary>
    /// Expansion point for serialization.
    /// </summary>
    public interface ISerializer
    {
        /// <summary>
        /// Serialize.
        /// </summary>
        /// <param name="item">Instance of item.</param>
        /// <typeparam name="T">Type of item.</typeparam>
        /// <returns>Bytes array.</returns>
        byte[] Serialize<T>(T item);

        /// <summary>
        /// Deserialize.
        /// </summary>
        /// <param name="bytes">Bytes array.</param>
        /// <typeparam name="T">Type of item.</typeparam>
        /// <returns>Instance of item.</returns>
        T Deserialize<T>(ArraySegment<byte> bytes);
    }
}
