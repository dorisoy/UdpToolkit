namespace UdpToolkit.Serialization
{
    using System;
    using System.Buffers;

    /// <summary>
    /// Expansion point for serialization.
    /// </summary>
    public interface ISerializer
    {
        /// <summary>
        /// Serialize object to buffer.
        /// </summary>
        /// <param name="buffer">Buffer writer.</param>
        /// <param name="item">Object instance.</param>
        /// <typeparam name="T">Type of object.</typeparam>
        void Serialize<T>(IBufferWriter<byte> buffer, T item);

        /// <summary>
        /// Deserialize from buffer to an existing object.
        /// </summary>
        /// <param name="buffer">Read only buffer.</param>
        /// <param name="item">Object instance.</param>
        /// <typeparam name="T">Type of object.</typeparam>
        /// <returns>Instance of object.</returns>
        T Deserialize<T>(ReadOnlySpan<byte> buffer, T item);
    }
}
