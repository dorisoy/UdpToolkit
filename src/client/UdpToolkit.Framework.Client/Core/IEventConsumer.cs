namespace UdpToolkit.Framework.Client.Core
{
    using System;
    using UdpToolkit.Core;

    public interface IEventConsumer
    {
        RpcDescriptorId RpcDescriptorId { get; }

        void Enqueue(ArraySegment<byte> payload);
    }
}