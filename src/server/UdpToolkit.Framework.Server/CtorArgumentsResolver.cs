namespace UdpToolkit.Framework.Server
{
    using System;
    using System.Collections.Generic;
    using UdpToolkit.Framework.Server.Core;

    public class CtorArgumentsResolver : ICtorArgumentsResolver
    {
        private readonly IContainer _container;

        public CtorArgumentsResolver(IContainer container)
        {
            _container = container;
        }

        public IEnumerable<object> GetInstances(IReadOnlyCollection<Type> types)
        {
            return _container.GetInstances(types);
        }
    }
}