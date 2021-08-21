namespace UdpToolkit.Framework.Contracts
{
    using System;
    using UdpToolkit.Framework.Contracts.Settings;

    public interface IHostBuilder
    {
        IHostBuilder ConfigureNetwork(
            Action<INetworkSettings> configurator);

        IHostBuilder ConfigureHost(
            HostSettings settings,
            Action<HostSettings> configurator);

        IHostBuilder ConfigureHostClient(
            Action<HostClientSettings> configurator);

        IHostBuilder BootstrapWorker(IHostWorker hostWorker);

        IHost Build();
    }
}