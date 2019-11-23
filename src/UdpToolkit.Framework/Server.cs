using System.Threading.Tasks;
using UdpToolkit.Core;

namespace UdpToolkit.Framework
{
    public sealed class Server : IServer
    {
        private readonly IUdpPacketsProcessor _udpPacketsProcessor;
        
        public Server(IUdpPacketsProcessor udpPacketsProcessor)
        {
            _udpPacketsProcessor = udpPacketsProcessor;
        }
        
        public Task RunAsync()
        {
            return _udpPacketsProcessor.RunAsync();
        }
    }
}