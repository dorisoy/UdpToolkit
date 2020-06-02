namespace UdpToolkit.Framework.Server.Di.Autofac
{
    using System;
    using global::Autofac;
    using UdpToolkit.Framework.Server.Core;

    public static class AutofacHost
    {
        public static IServerHostBuilder CreateServerBuilder(Action<ContainerBuilder> configurator)
        {
            return new AutofacServerHostBuilder(
                serverSettings: new ServerSettings(),
                configurator: configurator);
        }
    }
}
