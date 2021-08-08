namespace UdpToolkit.Network.Contracts.Sockets
{
    using System;

    public interface ISocket : IDisposable
    {
        IpV4Address GetLocalIp();

        int ReceiveFrom(ref IpV4Address address, byte[] buffer, int length);

        int Send(ref IpV4Address address, byte[] buffer, int length);

        int Bind(ref IpV4Address address);

        int Poll(long timeout);

        int SetNonBlocking();

        void Close();
    }
}