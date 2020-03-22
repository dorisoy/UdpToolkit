namespace UdpToolkit.Framework.Events
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class EventDescriptorNotFoundException : Exception
    {
        public EventDescriptorNotFoundException()
        {
        }

        public EventDescriptorNotFoundException(string message)
            : base(message)
        {
        }

        public EventDescriptorNotFoundException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected EventDescriptorNotFoundException(SerializationInfo info, StreamingContext context)
        {
        }
    }
}