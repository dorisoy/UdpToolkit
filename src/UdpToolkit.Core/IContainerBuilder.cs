using System;

namespace UdpToolkit.Core
{
    public interface IContainerBuilder
    {
        IContainerBuilder RegisterSingleton<TInterface, TService>(TService instance)
            where TService : TInterface;
        
        IContainerBuilder RegisterSingleton<TInterface, TService>(Func<TService> factory)
            where TService : TInterface;

        IContainer Build();
    }
}