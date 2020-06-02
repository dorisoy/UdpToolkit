namespace UdpToolkit.Framework.Server.Rpcs
{
    using System;
    using UdpToolkit.Annotations;

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class RpcAttribute : Attribute
    {
        public RpcAttribute(byte rpcId, UdpChannel udpChannel)
        {
            RpcId = rpcId;
            UdpChannel = udpChannel;
        }

        public byte RpcId { get; }

        public UdpChannel UdpChannel { get; }
    }
}