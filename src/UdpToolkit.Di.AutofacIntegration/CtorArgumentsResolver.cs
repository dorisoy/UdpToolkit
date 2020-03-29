namespace UdpToolkit.Di.AutofacIntegration
{
    using System;
    using System.Collections.Generic;
    using Autofac;
    using UdpToolkit.Core;

    public sealed class CtorArgumentsResolver : ICtorArgumentsResolver
    {
        private readonly ILifetimeScope _lifetimeScope;

        public CtorArgumentsResolver(ILifetimeScope lifetimeScope)
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