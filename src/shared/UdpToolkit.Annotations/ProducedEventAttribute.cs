namespace UdpToolkit.Annotations
{
    using System;

    [AttributeUsage(validOn: AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class ProducedEventAttribute : EventBaseAttribute
    {
        public ProducedEventAttribute(
            byte hubId,
            byte rpcId,
            UdpChannel udpChannel)
            : base(hubId, rpcId, udpChannel)
        {
        }
    }
}
