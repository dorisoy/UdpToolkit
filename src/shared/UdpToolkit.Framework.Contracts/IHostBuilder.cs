namespace UdpToolkit.Framework.Contracts
{
    using System;
    using UdpToolkit.Framework.Contracts.Settings;

    /// <summary>
    /// Host builder.
    /// </summary>
    public interface IHostBuilder
    {
        /// <summary>
        /// Network configuration.
        /// </summary>
        /// <param name="configurator">Lambda for configuring network.</param>
        /// <returns>HostBuilder.</returns>
        IHostBuilder ConfigureNetwork(
            Action<INetworkSettings> configurator);

        /// <summary>
        /// Host configuration.
        /// </summary>
        /// <param name="settings">Host settings instance.</param>
        /// <param name="configurator">Lambda for configuring host.</param>
        /// <returns>HostBuilder.</returns>
        IHostBuilder ConfigureHost(
            HostSettings settings,
            Action<HostSettings> configurator);

        /// <summary>
        /// Host client configuration.
        /// </summary>
        /// <param name="configurator">Lambda for configuring host client.</param>
        /// <returns>HostBuilder.</returns>
        IHostBuilder ConfigureHostClient(
            Action<HostClientSettings> configurator);

        /// <summary>
        /// Bootstrapping the generated host worker.
        /// </summary>
        /// <param name="hostWorker">HostWorker instance.</param>
        /// <returns>HostBuilder.</returns>
        IHostBuilder BootstrapWorker(IHostWorker hostWorker);

        /// <summary>
        /// Building the host with provided settings.
        /// </summary>
        /// <returns>
        /// Host instance.
        /// </returns>
        IHost Build();
    }
}