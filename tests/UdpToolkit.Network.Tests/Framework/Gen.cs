namespace UdpToolkit.Network.Tests.Framework
{
    using System;
    using System.Linq;
    using Bogus;
    using UdpToolkit.Network.Contracts.Protocol;

    public static class Gen
    {
        private static readonly Random Random = new Random();
        private static readonly Randomizer Randomizer = new Randomizer();

        public static int RandomInt(int min = 0, int max = 1000)
        {
            return Randomizer.Int(min, max);
        }

        public static byte RandomByte(byte min = 0, byte max = byte.MaxValue)
        {
            return Randomizer.Byte(min, max);
        }

        public static ushort RandomUshort(ushort min = 0, ushort max = ushort.MaxValue)
        {
            return Randomizer.UShort(min, max);
        }

        public static uint RandomUint(uint min = 0, uint max = uint.MaxValue)
        {
            return Randomizer.UInt(min, max);
        }

        public static T RandomEnum<T>()
            where T : struct, Enum
        {
            return Randomizer.Enum<T>();
        }

        public static Guid RandomGuid() => Guid.NewGuid();

        public static NetworkHeader[] GenerateRandomPackets(int count = 100)
        {
            return Enumerable.Range(0, count)
                .Select(_ => GenerateRandomPacket())
                .ToArray();
        }

        public static NetworkHeader GenerateRandomPacket()
        {
            return new NetworkHeader(
                channelId: Gen.RandomByte(),
                id: Gen.RandomUshort(),
                acks: Gen.RandomUint(),
                connectionId: Gen.RandomGuid(),
                packetType: Gen.RandomEnum<PacketType>(),
                dataType: Gen.RandomByte());
        }

        public static byte[] GenerateRandomBytes(int size)
        {
            var buffer = new byte[size];
            Random.NextBytes(buffer);
            return buffer;
        }
    }
}