namespace UdpToolkit.Core
{
    using System;
    using System.Threading.Tasks;

    public interface IHost : IDisposable
    {
        Task RunAsync();

        Task StopAsync();
    }
}
