using System;
using UdpToolkit.Framework.Rpcs;
using UdpToolkit.Network.Clients;

namespace UdpToolkit.Framework.Events
{
    public readonly struct EventDescriptor
    {
        public RpcDescriptorId RpcDescriptorId { get; }

        public UdpMode UdpMode { get; }

        public Type EventType { get; }

        public EventDescriptor(
            RpcDescriptorId rpcDescriptorId,
            UdpMode udpMode, 
            Type eventType)
        {
            RpcDescriptorId = rpcDescriptorId;
            UdpMode = udpMode;
            EventType = eventType;
        }
    }
}