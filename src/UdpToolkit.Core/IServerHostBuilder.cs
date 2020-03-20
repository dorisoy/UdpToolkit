using System;

namespace UdpToolkit.Core
{
    public interface IServerHostBuilder
    {
        IServerHostBuilder Configure(Action<ServerSettings> configurator);
        IServerHostBuilder ConfigureServices(Action<IContainerBuilder> configurator);
        IServerHost Build();
    }
}