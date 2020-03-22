namespace UdpToolkit.Core
{
    using System;
    using System.Collections.Generic;

    public interface IContainer
    {
        TInstance GetInstance<TInstance>();

        IEnumerable<object> GetInstances(IReadOnlyCollection<Type> types);
    }
}