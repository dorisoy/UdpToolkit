namespace UdpToolkit.Benchmarks.Fakes
{
    using System;
    using System.Linq;
    using UdpToolkit.Network.Contracts.Sockets;

    internal class FakeSocket : ISocket
    {
        private static readonly byte[] Bytes = Enumerable.Range(0, 100).Select(_ => (byte)_).ToArray();
        private readonly IpV4Address _localIp;
        private int _iterations = 0;

        internal FakeSocket(IpV4Address localIp)
        {
            _localIp = localIp;
        }

        public IpV4Address GetLocalIp()
        {
            return _localIp;
        }

        public int Send(ref IpV4Address address, byte[] buffer, int length)
        {
            throw new NotImplementedException();
        }

        public int ReceiveFrom(ref IpV4Address address, byte[] buffer, int length)
        {
            if (_iterations == 0)
            {
                _iterations++;
                Bytes.AsSpan().CopyTo(destination: buffer);
                return Bytes.Length;
            }

            return 0;
        }

        public int Bind(ref IpV4Address address)
        {
            return 0;
        }

        public int Poll(long timeout)
        {
            return 1;
        }

        public int SetNonBlocking()
        {
            return 1;
        }

        public void Close()
        {
        }

        public void Dispose()
        {
        }
    }
}