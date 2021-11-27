namespace Serializers
{
    using System;
    using System.Buffers;
    using Google.Protobuf;
    using UdpToolkit.Serialization;

    public class ProtobufSerializer : ISerializer
    {
        public void Serialize<T>(IBufferWriter<byte> buffer, T item)
        {
            if (item is IMessage message)
            {
                message.WriteTo(buffer);
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        public T Deserialize<T>(ReadOnlySpan<byte> buffer, T item)
        {
            if (item is IMessage message)
            {
                message.MergeFrom(buffer);

                return item;
            }

            throw new NotSupportedException();
        }
    }
}