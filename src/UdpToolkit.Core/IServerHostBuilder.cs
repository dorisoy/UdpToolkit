namespace UdpToolkit.Core
{
    using System;

    public interface IServerHostBuilder
    {
        IServerHostBuilder Configure(Action<ServerSettings> configurator);

        IServerHostBuilder ConfigureServices(Action<IContainerBuilder> configurator);

        IServerHostBuilder Use(Action<IPipelineBuilder> configurator);

        IServerHost Build();
    }
}