﻿namespace UdpToolkit.Serialization.MsgPack
{
    using System;
    using MessagePack;
    using MessagePack.Resolvers;
    using UdpToolkit.Core;

    public sealed class Serializer : ISerializer
    {
        public byte[] Serialize<T>(T @event)
        {
            return MessagePackSerializer.Serialize(@event, ContractlessStandardResolver.Instance);
        }

        public object Deserialize(Type type, ArraySegment<byte> bytes)
        {
            return MessagePackSerializer.NonGeneric.Deserialize(type, bytes, ContractlessStandardResolver.Instance);
        }

        public T Deserialize<T>(ArraySegment<byte> bytes)
        {
            return MessagePackSerializer.Deserialize<T>(bytes);
        }
    }
}