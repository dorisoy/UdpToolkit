namespace UdpToolkit.Network.Sockets
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Net;
    using System.Net.Sockets;
    using UdpToolkit.Network.Contracts.Sockets;

    /// <summary>
    /// Managed .NET socket implementation of ISocket.
    /// </summary>
    public sealed class ManagedSocket : ISocket
    {
        private readonly Socket _socket;
        private readonly List<Socket> _sockets;

        private EndPoint _remoteIp = new IPEndPoint(IPAddress.Any, 0);
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="ManagedSocket"/> class.
        /// </summary>
        /// <param name="socket">Instance of .NET socket.</param>
        public ManagedSocket(
            Socket socket)
        {
            _socket = socket;
            _sockets = new List<Socket>();
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="ManagedSocket"/> class.
        /// </summary>
        [ExcludeFromCodeCoverage]
        ~ManagedSocket()
        {
            Dispose(false);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
        }

        /// <inheritdoc />
        public IpV4Address GetLocalIp()
        {
            return ((IPEndPoint)_socket.LocalEndPoint).ToIp();
        }

        /// <inheritdoc />
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
            address = new IpV4Address(ip.Address.ToInt(), (ushort)ip.Port);
            return bytes;
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
        public int Bind(ref IpV4Address address)
        {
            _socket.Bind(new IPEndPoint(new IPAddress(address.Address), address.Port));
            return 1;
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
        public int SetNonBlocking()
        {
            _socket.Blocking = false;
            return 1;
        }

        /// <inheritdoc />
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

            _disposed = true;
        }
    }
}