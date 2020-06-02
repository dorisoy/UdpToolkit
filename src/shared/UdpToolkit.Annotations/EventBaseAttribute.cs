namespace UdpToolkit.Annotations
{
    using System;

    public abstract class EventBaseAttribute : Attribute
    {
        protected EventBaseAttribute(
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