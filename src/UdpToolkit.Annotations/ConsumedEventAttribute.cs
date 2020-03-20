using System;

namespace UdpToolkit.Annotations
{
    [AttributeUsage(validOn: AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class ConsumedEventAttribute : Attribute, IEventAttribute
    {
        public byte HubId { get; }
        public byte RpcId { get; }
        public UdpChannel UdpChannel { get; }

        public ConsumedEventAttribute(
            byte hubId, 
            byte rpcId, 
            UdpChannel udpChannel)
        {
            HubId = hubId;
            RpcId = rpcId;
            UdpChannel = udpChannel;
        }
    }
}