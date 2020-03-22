namespace UdpToolkit.Framework.Events
{
    using System;
    using UdpToolkit.Framework.Rpcs;
    using UdpToolkit.Network.Clients;

    public readonly struct EventDescriptor
    {
        public EventDescriptor(
            RpcDescriptorId rpcDescriptorId,
            UdpMode udpMode,
            Type eventType)
        {
            RpcDescriptorId = rpcDescriptorId;
            UdpMode = udpMode;
            EventType = eventType;
        }

        public RpcDescriptorId RpcDescriptorId { get; }

        public UdpMode UdpMode { get; }

        public Type EventType { get; }
    }
}