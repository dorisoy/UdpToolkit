namespace UdpToolkit.Framework.Events
{
    using System;
    using UdpToolkit.Core;
    using UdpToolkit.Network.Clients;

    public readonly struct EventDescriptor
    {
        public EventDescriptor(
            RpcDescriptorId rpcDescriptorId,
            UdpToolkit.Network.Clients.UdpMode udpMode,
            Type eventType)
        {
            RpcDescriptorId = rpcDescriptorId;
            UdpMode = udpMode;
            EventType = eventType;
        }

        public RpcDescriptorId RpcDescriptorId { get; }

        public UdpToolkit.Network.Clients.UdpMode UdpMode { get; }

        public Type EventType { get; }
    }
}