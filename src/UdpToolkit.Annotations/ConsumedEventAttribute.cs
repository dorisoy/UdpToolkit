namespace UdpToolkit.Annotations
{
    using System;

    [AttributeUsage(validOn: AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class ConsumedEventAttribute : Attribute, IEventAttribute
    {
        public ConsumedEventAttribute(
            byte hubId,
            byte rpcId,
            UdpChannel udpChannel)
        {
            HubId = hubId;
            RpcId = rpcId;
            UdpChannel = udpChannel;
        }

        public byte HubId { get; }

        public byte RpcId { get; }

        public UdpChannel UdpChannel { get; }
    }
}