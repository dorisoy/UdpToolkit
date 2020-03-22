namespace UdpToolkit.Core
{
    using System;
    using UdpToolkit.Framework.Rpcs;

    public interface IEventConsumer
    {
        RpcDescriptorId RpcDescriptorId { get; }

        void Enqueue(ArraySegment<byte> payload);
    }
}