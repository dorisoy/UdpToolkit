using System;

namespace UdpToolkit.Framework
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
    public class RpcAttribute : Attribute
    {
        public byte RpcId { get; }

        public RpcAttribute(byte rpcId)
        {
            RpcId = rpcId;
        }
    }
}