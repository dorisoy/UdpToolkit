namespace UdpToolkit.Framework.Hosts
{
    using UdpToolkit.Core;
    using UdpToolkit.Framework.Hosts.Client;
    using UdpToolkit.Framework.Hosts.Server;
    using UdpToolkit.Framework.Pipelines;

    public static class Host
    {
        public static IServerHostBuilder CreateServerBuilder(IContainerBuilder containerBuilder)
        {
            return new ServerHostBuilder(
                settings: new ServerSettings(),
                containerBuilder: containerBuilder);
        }

        public static IClientHostBuilder CreateClientBuilder(IContainerBuilder containerBuilder)
        {
            return new ClientHostHostBuilder(
                settings: new ClientSettings(),
                containerBuilder: containerBuilder);
        }
    }
}