using System;
using System.Threading.Tasks;

namespace UdpToolkit.Core
{
    public interface IUdpSender : IDisposable
    {
        Task StartSending();

        Task Send(OutputUdpPacket outputUdpPacket);
    }
}