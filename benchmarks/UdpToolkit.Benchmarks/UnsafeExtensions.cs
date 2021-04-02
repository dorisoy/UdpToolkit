namespace UdpToolkit.Benchmarks
{
    using System;
    using System.Runtime.CompilerServices;

    public static class UnsafeExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void WriteWithoutSpan<T>(ref byte[] destination, ref T value)
            where T : unmanaged
        {
            fixed (T* pValue = &value)
            {
                fixed (byte* pDestination = destination)
                {
                    Unsafe.CopyBlock(destination: pDestination, source: pValue, byteCount: (uint)sizeof(T));
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void Write<T>(this byte[] destination, T value)
            where T : unmanaged
        {
            var pointer = Unsafe.AsPointer(ref value);
            var source = new Span<byte>(pointer, sizeof(T));
            source.CopyTo(destination.AsSpan());
        }
    }
}