namespace UdpToolkit.Network.Serialization
{
    using System;
    using System.Runtime.CompilerServices;

    internal static class UnsafeSerialization
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void Write<T>(byte[] buffer, T value)
            where T : unmanaged
        {
            var pointer = Unsafe.AsPointer(ref value);
            var source = new Span<byte>(pointer, sizeof(T));
            source.CopyTo(buffer.AsSpan());
        }

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