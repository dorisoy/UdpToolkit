using System.Threading.Tasks;

namespace UdpToolkit.Core
{
    public interface IUdpPacketsProcessor
    {
        Task RunAsync();
    }
}
