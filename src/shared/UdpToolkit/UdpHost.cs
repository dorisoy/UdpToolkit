namespace UdpToolkit
{
    using UdpToolkit.Framework.Contracts;
    using UdpToolkit.Framework.Contracts.Settings;

    /// <summary>
    /// Start point for building host.
    /// </summary>
    public static class UdpHost
    {
        /// <summary>
        /// Create host builder.
        /// </summary>
        /// <returns>
        /// Host builder instance.
        /// </returns>
        public static IHostBuilder CreateHostBuilder()
        {
            return new HostBuilder(
                hostClientSettings: new HostClientSettings(),
                networkSettings: new NetworkSettings());
        }
    }
}
