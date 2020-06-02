namespace UdpToolkit.Framework.Server.Di.Autofac
{
    using System;
    using System.Collections.Generic;
    using global::Autofac;
    using UdpToolkit.Framework.Server.Core;

    public class AutofacCtorArgumentsResolver : ICtorArgumentsResolver
    {
        private readonly ILifetimeScope _lifetimeScope;

        public AutofacCtorArgumentsResolver(ILifetimeScope lifetimeScope)
        {
            _lifetimeScope = lifetimeScope;
        }

        public IEnumerable<object> GetInstances(IReadOnlyCollection<Type> types)
        {
            foreach (var type in types)
            {
                yield return _lifetimeScope.Resolve(type);
            }
        }
    }
}