using System;
using UdpToolkit.Core;

namespace UdpToolkit.Tests.Fakes
{
    public class FakeSerializer : ISerializer
    {
        public byte[] Serialize<T>(T @event)
        {
            throw new NotImplementedException();
        }

        public object Deserialize(Type type, ArraySegment<byte> bytes)
        {
            throw new NotImplementedException();
        }

        public T Deserialize<T>(ArraySegment<byte> bytes)
        {
            throw new NotImplementedException();
        }
    }
}