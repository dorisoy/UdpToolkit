using System;

namespace UdpToolkit.Annotations
{
    [AttributeUsage(validOn: AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class ProducedEventAttribute : Attribute, IEventAttribute
    {
        public byte HubId { get; }
        public byte RpcId { get; }
        public UdpChannel UdpChannel { get; }

        public ProducedEventAttribute(
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
