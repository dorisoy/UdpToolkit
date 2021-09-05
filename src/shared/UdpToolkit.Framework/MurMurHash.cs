#pragma warning disable SA1306
#pragma warning disable SA1310
#pragma warning disable SA1300
#pragma warning disable S907
namespace UdpToolkit.Framework
{
    using System;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Algorithm description:
    /// https://en.wikipedia.org/wiki/MurmurHash
    /// Original version ported to C# from kafka:
    /// https://github.com/apache/kafka/blob/2.8/streams/src/main/java/org/apache/kafka/streams/state/internals/Murmur3.java#L140.
    /// </summary>
    public static class MurMurHash
    {
        private static uint C1_32 = 0xcc9e2d51;
        private static uint N_32 = 0xe6546b64;
        private static uint C2_32 = 0x1b873593;
        private static uint M_32 = 5;

        private static int R2_32 = 13;
        private static int R1_32 = 15;

        /// <summary>
        /// MurMur hash implementation.
        /// </summary>
        /// <param name="guid">Any guid.</param>
        /// <returns>Hash.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Hash3_x86_32(Guid guid)
        {
            var bytes = guid.ToByteArray();
            return Hash3_x86_32(bytes, 0, (uint)bytes.Length, 0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int Hash3_x86_32(byte[] data, uint offset, uint length, uint seed)
        {
            uint hash = seed;
            uint nblocks = length >> 2;

            // body
            for (uint i = 0; i < nblocks; i++)
            {
                uint i4 = i << 2;
                var k = (uint)((data[offset + i4] & 0xff)
                        | ((data[offset + i4 + 1] & 0xff) << 8)
                        | ((data[offset + i4 + 2] & 0xff) << 16)
                        | ((data[offset + i4 + 3] & 0xff) << 24));

                hash = Mix32(k, hash);
            }

            // tail
            uint idx = nblocks << 2;
            uint k1 = 0;
            switch (length - idx)
            {
                case 3:
                    k1 ^= (uint)data[offset + idx + 2] << 16;
                    goto case 2;
                case 2:
                    k1 ^= (uint)data[offset + idx + 1] << 8;
                    goto case 1;
                case 1:
                    k1 ^= data[offset + idx];

                    // mix functions
                    k1 *= C1_32;
                    k1 = RotateLeft(k1, R1_32);
                    k1 *= C2_32;
                    hash ^= k1;
                    break;
            }

            return Fmix32(length, hash);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int Fmix32(uint length, uint hash)
        {
            hash ^= length;
            hash ^= hash >> 16;
            hash *= 0x85ebca6b;
            hash ^= hash >> 13;
            hash *= 0xc2b2ae35;
            hash ^= hash >> 16;

            return (int)(hash & int.MaxValue);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint Mix32(uint k, uint hash)
        {
            k *= C1_32;
            k = RotateLeft(k, R1_32);
            k *= C2_32;
            hash ^= k;
            return (RotateLeft(hash, R2_32) * M_32) + N_32;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint RotateLeft(uint original, int bits)
        {
            return (original << bits) | (original >> (32 - bits));
        }
    }
}
#pragma warning restore SA1306
#pragma warning restore SA1310
#pragma warning restore SA1300