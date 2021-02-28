namespace UdpToolkit.Network.Clients
{
    using System;
    using System.Threading.Tasks;
    using UdpToolkit.Network.Packets;

    public interface IUdpReceiver : IDisposable
    {
        Task<ValueTuple<NetworkPacket, bool>> ReceiveAsync();
    }
}
