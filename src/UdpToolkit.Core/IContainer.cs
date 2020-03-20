using System;
using System.Collections.Generic;

namespace UdpToolkit.Core
{
    public interface IContainer
    {
        TInstance GetInstance<TInstance>();

        IEnumerable<object> GetInstances(IReadOnlyCollection<Type> types);
    }
}