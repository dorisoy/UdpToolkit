namespace UdpToolkit.Framework.Hubs
{
    using System;

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class HubAttribute : Attribute
    {
        public HubAttribute(byte hubId)
        {
            HubId = hubId;
        }

        public byte HubId { get; }
    }
}