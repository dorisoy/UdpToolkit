namespace UdpToolkit.Framework.Server.Di
{
    using System;
    using System.Collections.Concurrent;
    using UdpToolkit.Framework.Server.Core;

    public class ContainerBuilder : IContainerBuilder
    {
        private readonly ConcurrentDictionary<Type, Func<object>> _registrations = new ConcurrentDictionary<Type, Func<object>>();

        public IContainerBuilder RegisterSingleton<TInterface, TService>(TService instance)
            where TService : TInterface
        {
            var lazy = new Lazy<TService>(instance);
            _registrations.TryAdd(typeof(TInterface), () => lazy.Value);

            return this;
        }

        public IContainerBuilder RegisterSingleton<TInterface, TService>(Func<TService> factory)
            where TService : TInterface
        {
            var lazy = new Lazy<TService>(factory());
            _registrations.TryAdd(typeof(TInterface), () => lazy.Value);

            return this;
        }

        public IContainer Build()
        {
            return new Container(
                registrations: new ConcurrentDictionary<Type, Func<object>>(_registrations));
        }
    }
}