using System;
using UdpToolkit.Framework.Rpcs;

namespace UdpToolkit.Core
{
    public interface IEventConsumer
    {
        RpcDescriptorId RpcDescriptorId { get; }

        void Enqueue(ArraySegment<byte> payload);
    }
}