using System;
using System.Threading.Tasks;

namespace UdpToolkit.Core
{
    public interface IUdpReceiver : IDisposable
    {
        Task StartReceive();
    }
}
