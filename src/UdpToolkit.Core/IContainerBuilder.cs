namespace UdpToolkit.Core
{
    using System;

    public interface IContainerBuilder
    {
        IContainerBuilder RegisterSingleton<TInterface, TService>(TService instance)
            where TService : class, TInterface;

        IContainerBuilder RegisterSingleton<TInterface, TService>(TService instance, string name)
            where TService : class, TInterface;

        IContainerBuilder RegisterSingleton<TInterface, TService>(Func<IRegistrationContext, TService> func)
            where TService : class, TInterface;

        IContainerBuilder RegisterSingleton<TInterface, TService>(Func<TService> factory)
            where TService : class, TInterface;

        IContainer Build();
    }
}