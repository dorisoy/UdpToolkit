namespace UdpToolkit.Core
{
    using System.Threading.Tasks;

    public interface IHost
    {
        Task RunAsync();
    }
}
