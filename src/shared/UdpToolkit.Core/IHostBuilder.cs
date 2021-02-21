namespace UdpToolkit.Core
{
    using System;

    public interface IHostBuilder
    {
        IHostBuilder ConfigureHost(Action<HostSettings> configurator);

        IHostBuilder ConfigureHostClient(Action<HostClientSettings> configurator);

        IHost Build();
    }
}