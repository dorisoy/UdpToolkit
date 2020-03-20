using System;
using System.Runtime.Serialization;

namespace UdpToolkit.Framework.Events
{
    [Serializable]
    public class DuplicateEventTypeException : Exception
    {
        public DuplicateEventTypeException()
        {
        }

        public DuplicateEventTypeException(string message)
            : base(message)
        {
        }

        public DuplicateEventTypeException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected DuplicateEventTypeException(SerializationInfo info, StreamingContext context)
        {
        }
    }
}