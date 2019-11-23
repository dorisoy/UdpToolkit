using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace UdpToolkit.Core
{
    public class MethodDescriptor
    {
        public MethodDescriptor(
            IEnumerable<Type> arguments,
            byte hubId,
            byte methodId,
            Type returnType,
            Type hubType, 
            MethodInfo methodInfo)
            
        {
            Arguments = arguments;
            HubId = hubId;
            MethodId = methodId;
            ReturnType = returnType;
            HubType = hubType;
            MethodInfo = methodInfo;
        }

        public byte MethodId { get; }

        public IEnumerable<Type> Arguments { get; }
        public byte HubId { get; }

        public Type ReturnType { get; }
        public Type HubType { get; }

        public MethodInfo MethodInfo { get; }
    }
}