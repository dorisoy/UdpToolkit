using System.Threading.Tasks;
using UdpToolkit.Core;
using UdpToolkit.Framework.Hosts;
using UdpToolkit.Serialization.MsgPack;

namespace SimpleUdp.Server
{
    public class Program
    {
        public static async Task Main()
        {
            var server = BuildServer();

            await server.RunAsync();
        }

        private static IServerHost BuildServer() =>
            Host
                .CreateServerBuilder()
                .Configure(settings =>
                {
                    settings.ServerHost = "0.0.0.0";
                    settings.InputPorts = new[] { 7000, 7001 };
                    settings.OutputPorts = new[] { 8000, 8001 };
                    settings.ProcessWorkers = 2;
                    settings.Serializer = new Serializer();
                })
                .ConfigureServices(builder =>
                {
                    builder.RegisterSingleton<IService, Service>(() => new Service());
                })
                .Build();
    }
}
