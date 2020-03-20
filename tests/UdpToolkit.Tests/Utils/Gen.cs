using System;
using Bogus;

namespace UdpToolkit.Tests.Utils
{
    public static class Gen
    {
        public static Faker Faker = new Faker();
        
        public static int GetRandomInt()
        {
            return Faker.Random.Int();
        }
        
        public static byte GetRandomByte()
        {
            return Faker.Random.Byte();
        }
        
        public static ushort GetRandomUshort(ushort min = ushort.MinValue, ushort max = ushort.MaxValue)
        {
            return Faker.Random.UShort(min, max);
        }
        
        public static uint GetRandomUint()
        {
            return Faker.Random.UInt();
        }
    }
}
