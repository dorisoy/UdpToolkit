namespace UdpToolkit.Network.Utils
{
    using System;
    using System.Buffers.Binary;
    using System.Runtime.InteropServices;

    /// <summary>
    /// Span utils.
    /// </summary>
    public static class SpanUtils
    {
        /// <summary>
        /// Backport GUID ctor for netstandard2.0.
        /// </summary>
        /// <param name="span">Span instance.</param>
        /// <returns>Guid instance.</returns>
        public static Guid ToGuid(ReadOnlySpan<byte> span)
        {
            if (BitConverter.IsLittleEndian)
            {
                return MemoryMarshal.Read<Guid>(span);
            }

            return new Guid(
                a: BinaryPrimitives.ReadInt32LittleEndian(span),
                b: BinaryPrimitives.ReadInt16LittleEndian(span.Slice(4)),
                c: BinaryPrimitives.ReadInt16LittleEndian(span.Slice(6)),
                d: span[8],
                e: span[9],
                f: span[10],
                g: span[11],
                h: span[12],
                i: span[13],
                j: span[14],
                k: span[15]);
        }
    }
}