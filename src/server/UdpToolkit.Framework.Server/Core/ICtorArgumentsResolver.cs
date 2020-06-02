namespace UdpToolkit.Framework.Server.Core
{
    using System;
    using System.Collections.Generic;

    public interface ICtorArgumentsResolver
    {
        IEnumerable<object> GetInstances(IReadOnlyCollection<Type> types);
    }
}