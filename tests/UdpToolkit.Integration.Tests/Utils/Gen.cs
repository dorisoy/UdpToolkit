namespace UdpToolkit.Integration.Tests.Utils
{
    using System;
    using System.Linq;
    using Bogus;

    public static class Gen
    {
        public static readonly Faker Faker = new Faker();

        public static int RandomInt()
        {
            return Faker.Random.Int();
        }

        public static int[] GenerateUdpPorts(int count) => Enumerable
            .Range(0, count)
            .ToArray();

        public static TimeSpan RandomTimeSpanFromMinutes()
        {
            return TimeSpan.FromMinutes(Faker.Random.Int());
        }

        public static byte RandomByte()
        {
            return Faker.Random.Byte();
        }

        public static ushort RandomUshort(ushort min = ushort.MinValue, ushort max = ushort.MaxValue)
        {
            return Faker.Random.UShort(min, max);
        }

        public static uint RandomUint()
        {
            return Faker.Random.UInt();
        }

        public static string RandomString()
        {
            return Faker.Random.String();
        }
    }
}
