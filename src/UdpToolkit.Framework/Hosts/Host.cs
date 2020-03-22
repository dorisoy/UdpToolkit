namespace UdpToolkit.Framework.Hosts
{
    using UdpToolkit.Core;
    using UdpToolkit.Framework.Di;
    using UdpToolkit.Framework.Hosts.Client;
    using UdpToolkit.Framework.Hosts.Server;

    public static class Host
    {
        public static IServerHostBuilder CreateServerBuilder()
        {
            return new ServerHostBuilder(
                settings: new ServerSettings(),
                containerBuilder: new ContainerBuilder());
        }

        public static IClientHostBuilder CreateClientBuilder()
        {
            return new ClientHostHostBuilder(
                settings: new ClientSettings());
        }
    }
}