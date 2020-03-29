namespace UdpToolkit.Di.AutofacIntegration
{
    using System;
    using Autofac;
    using UdpToolkit.Core;

    public sealed class ContainerBuilder : IContainerBuilder
    {
        private readonly Autofac.ContainerBuilder _containerBuilder;

        public ContainerBuilder()
        {
            _containerBuilder = new Autofac.ContainerBuilder();
        }

        public IContainerBuilder RegisterSingleton<TInterface, TService>(TService instance)
            where TService : class, TInterface
        {
            _containerBuilder
                .RegisterInstance(instance)
                .As<TInterface>()
                .SingleInstance();

            return this;
        }

        public IContainerBuilder RegisterSingleton<TInterface, TService>(TService instance, string name)
            where TService : class, TInterface
        {
            _containerBuilder
                .RegisterInstance(instance)
                .As<TInterface>()
                .SingleInstance()
                .Named<TInterface>(name);

            return this;
        }

        public IContainerBuilder RegisterSingleton<TInterface, TService>(Func<IRegistrationContext, TService> func)
            where TService : class, TInterface
        {
            _containerBuilder.Register((context) => func(new RegistrationContext(context)))
                .As<TInterface>()
                .SingleInstance();

            return this;
        }

        public IContainerBuilder RegisterSingleton<TInterface, TService>(Func<TService> factory)
            where TService : class, TInterface
        {
            _containerBuilder
                .RegisterInstance(factory())
                .As<TInterface>()
                .SingleInstance();

            return this;
        }

        public UdpToolkit.Core.IContainer Build()
        {
            _containerBuilder
                .RegisterType<CtorArgumentsResolver>()
                .As<ICtorArgumentsResolver>()
                .SingleInstance();

            var container = _containerBuilder.Build();

            return new Container(container: container);
        }
    }
}
