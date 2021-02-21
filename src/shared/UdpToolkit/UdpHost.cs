namespace UdpToolkit
{
    using UdpToolkit.Core;

    public static class UdpHost
    {
        public static IHostBuilder CreateHostBuilder()
        {
            return new HostBuilder(
                hostSettings: new HostSettings(),
                hostClientSettings: new HostClientSettings());
        }
    }
}
