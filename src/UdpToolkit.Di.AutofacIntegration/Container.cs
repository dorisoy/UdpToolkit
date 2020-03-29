namespace UdpToolkit.Di.AutofacIntegration
{
    using System;
    using System.Collections.Generic;
    using Autofac;

    public sealed class Container : UdpToolkit.Core.IContainer, IDisposable
    {
        private readonly Autofac.IContainer _container;

        public Container(Autofac.IContainer container)
        {
            _container = container;
        }

        public TInstance GetInstance<TInstance>()
        {
            return _container.Resolve<TInstance>();
        }

        public IEnumerable<object> GetInstances(IReadOnlyCollection<Type> types)
        {
            foreach (var type in types)
            {
                yield return _container.Resolve(type);
            }
        }

        public void Dispose()
        {
            _container?.Dispose();
        }
    }
}