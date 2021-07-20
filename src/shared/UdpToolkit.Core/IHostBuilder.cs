namespace UdpToolkit.Core
{
    using System;
    using UdpToolkit.Core.Settings;

    public interface IHostBuilder
    {
        IHostBuilder ConfigureNetwork(Action<NetworkSettings> configurator);

        IHostBuilder ConfigureHost(Action<HostSettings> configurator);

        IHostBuilder ConfigureHostClient(Action<HostClientSettings> configurator);

        IHost Build();
    }
}