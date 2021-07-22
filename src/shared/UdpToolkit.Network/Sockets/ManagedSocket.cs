namespace UdpToolkit.Network.Sockets
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Sockets;
    using UdpToolkit.Logging;

    public sealed class ManagedSocket : ISocket
    {
        private readonly IUdpToolkitLogger _logger;
        private readonly Socket _socket;
        private readonly List<Socket> _sockets;

        private EndPoint _remoteIp = new IPEndPoint(IPAddress.Any, 0);
        private bool _disposed;

        public ManagedSocket(
            Socket socket,
            IUdpToolkitLogger logger)
        {
            _socket = socket;
            _logger = logger;
            _sockets = new List<Socket>();
        }

        ~ManagedSocket()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        public IpV4Address GetLocalIp()
        {
            return ((IPEndPoint)_socket.LocalEndPoint).ToIp();
        }

        public int ReceiveFrom(ref IpV4Address address, byte[] buffer, int length)
        {
            if (_sockets.Count == 0)
            {
                _sockets.Add(_socket);
            }

            // https://github.com/dotnet/runtime/issues/47342
            // Before doing blocking call 'ReceiveFrom' we should check ready data in the socket to preventing deadlock for the correct 'Dispose' call.
            Socket.Select(_sockets, null, null, 1000);

            if (_sockets.Count == 0)
            {
                return 0;
            }

            var socket = _sockets[0];
            if (socket == null)
            {
                return 0;
            }

            var bytes = socket.ReceiveFrom(
                buffer: buffer,
                socketFlags: SocketFlags.None,
                remoteEP: ref _remoteIp);

            var ip = (IPEndPoint)_remoteIp;
            address.Address = ip.Address.ToInt();
            address.Port = (ushort)ip.Port;
            return bytes;
        }

        public int Send(ref IpV4Address address, byte[] buffer, int length)
        {
            try
            {
                return _socket.SendTo(buffer, 0, length, SocketFlags.None, address.ToIpEndPoint());
            }
            catch (ObjectDisposedException)
            {
                // ignore
            }

            return 0;
        }

        public int Bind(ref IpV4Address address)
        {
            _socket.Bind(new IPEndPoint(new IPAddress(address.Address), address.Port));
            return 1;
        }

        public int Poll(long timeout)
        {
            try
            {
                return _socket.Poll((int)timeout, SelectMode.SelectRead) ? 1 : 0;
            }
            catch (ObjectDisposedException)
            {
                // ignore
            }
            catch (SocketException ex) when (ex.ErrorCode == 10_004)
            {
                // ignore, windows specific exception
                //  "A blocking operation was interrupted by a call to WSACancelBlockingCall"
            }

            return 0;
        }

        public int SetNonBlocking()
        {
            _socket.Blocking = false;
            return 1;
        }

        public void Close()
        {
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _socket.Dispose();
            }

            _logger.Debug($"Managed socket disposed!");
            _disposed = true;
        }
    }
}