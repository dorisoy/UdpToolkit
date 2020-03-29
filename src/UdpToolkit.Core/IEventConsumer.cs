namespace UdpToolkit.Core
{
    using System;

    public interface IEventConsumer
    {
        RpcDescriptorId RpcDescriptorId { get; }

        void Enqueue(ArraySegment<byte> payload);
    }
}