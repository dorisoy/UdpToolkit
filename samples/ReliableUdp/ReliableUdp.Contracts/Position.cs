namespace ReliableUdp.Contracts
{
    using System;
    using ProtoBuf;
    using UdpToolkit.Framework;

    [ProtoContract]
    public sealed class Position : IDisposable
    {
        [Obsolete("Serialization only")]
        public Position()
        {
        }

        public Position(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        [ProtoMember(1)]
        public int X { get; }

        [ProtoMember(2)]
        public int Y { get; }

        [ProtoMember(3)]
        public int Z { get; }

        public void Dispose()
        {
            Console.WriteLine($"{this.GetType().Name} returned to pool.");
            ObjectsPool<Position>.Return(this);
        }
    }
}