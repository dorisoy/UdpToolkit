namespace UdpToolkit.Framework.Server.Core
{
    using System;

    public interface IServerHostBuilder
    {
        IServerHostBuilder Configure(Action<ServerSettings> configurator);

        IServerHostBuilder Use(Action<IPipelineBuilder> configurator);

        IServerHost Build();
    }
}