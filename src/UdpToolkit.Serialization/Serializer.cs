using System;
using MessagePack;
using MessagePack.Resolvers;
using UdpToolkit.Core;

namespace UdpToolkit.Serialization
{
    public class Serializer : ISerializer
    {
        public byte[] Serialize<T>(T @event)
        {
            return MessagePackSerializer.Serialize(@event, ContractlessStandardResolver.Instance);
        }

        public object Deserialize(Type type, ArraySegment<byte> bytes)
        {
            return MessagePackSerializer.NonGeneric.Deserialize(type, bytes, ContractlessStandardResolver.Instance);
        }
    }
}