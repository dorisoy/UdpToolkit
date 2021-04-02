namespace UdpToolkit.Benchmarks.Utils
{
    using System;
    using System.Buffers;
    using System.Net;
    using System.Net.Sockets;

    public class EventArgsWrapper : IResettable
    {
        private readonly SocketAsyncEventArgs _e;

        public EventArgsWrapper(
            EventHandler<SocketAsyncEventArgs> action)
        {
            _e = new SocketAsyncEventArgs();
            _e.Completed += action;
            var array = ArrayPool<byte>.Shared.Rent(10);
            array[0] = 1;
            array[1] = 2;
            array[2] = 3;
            array[3] = 4;
            array[4] = 5;
            _e.SetBuffer(array, 0, 10);
        }

        public SocketAsyncEventArgs SocketAsyncEventArgs => _e;

        public void Set(
            IPEndPoint ipEndPoint)
        {
            _e.SocketFlags = SocketFlags.None;
            _e.RemoteEndPoint = ipEndPoint;
        }

        public void Reset()
        {
            // _e.RemoteEndPoint = null;
        }
#nullable disable
    }
}