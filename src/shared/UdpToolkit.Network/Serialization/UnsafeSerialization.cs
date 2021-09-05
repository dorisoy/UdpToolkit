namespace UdpToolkit.Network.Serialization
{
    using System;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Internal utils class contains methods for fast GC-free binary serialization.
    /// </summary>
    internal static class UnsafeSerialization
    {
        /// <summary>
        /// Write data to byte buffer.
        /// </summary>
        /// <param name="buffer">Buffer.</param>
        /// <param name="value">Instance of unmanaged struct.</param>
        /// <typeparam name="T">User-defined type.</typeparam>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void Write<T>(byte[] buffer, T value)
            where T : unmanaged
        {
            var pointer = Unsafe.AsPointer(ref value);
            var source = new Span<byte>(pointer, sizeof(T));
            source.CopyTo(buffer.AsSpan());
        }

        /// <summary>
        /// Read data from byte buffer.
        /// </summary>
        /// <param name="buffer">Buffer.</param>
        /// <typeparam name="T">User-defined type.</typeparam>
        /// <returns>Instance of unmanaged struct with received data.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe T Read<T>(Span<byte> buffer)
            where T : unmanaged
        {
            var result = default(T);
            var pointer = Unsafe.AsPointer(ref result);
            var span = new Span<byte>(pointer, sizeof(T));
            buffer.CopyTo(span);
            return result;
        }
    }
}