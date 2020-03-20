using System;
using UdpToolkit.Annotations;

namespace UdpToolkit.Framework.Rpcs
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class RpcAttribute : Attribute
    {
        public byte RpcId { get; }
        public UdpChannel UdpChannel { get; }
        
        public RpcAttribute(byte rpcId, UdpChannel udpChannel)
        {
            RpcId = rpcId;
            UdpChannel = udpChannel;
        }
    }
}