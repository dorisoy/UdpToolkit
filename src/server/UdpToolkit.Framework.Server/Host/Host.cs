namespace UdpToolkit.Framework.Server.Host
{
    using System;
    using UdpToolkit.Framework.Server.Core;

    public static class Host
    {
        public static IServerHostBuilder CreateServerBuilder(Action<IContainerBuilder> configurator)
        {
            return new ServerHostBuilder(
                serverSettings: new ServerSettings(),
                configurator: configurator);
        }
    }
}