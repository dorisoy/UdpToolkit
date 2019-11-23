using System;

namespace UdpToolkit.Framework
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class HubAttribute : Attribute
    {
        public byte HubId { get; }

        public HubAttribute(byte hubId)
        {
            HubId = hubId;
        }       
    }
}