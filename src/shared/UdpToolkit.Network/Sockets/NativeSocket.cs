namespace UdpToolkit.Network.Sockets
{
    using System.Runtime.InteropServices;
    using UdpToolkit.Network.Contracts.Sockets;

    /// <summary>
    /// Native socket implementation of ISocket.
    /// </summary>
    public sealed class NativeSocket : ISocket
    {
        private const string LibName = "udp_toolkit_native";
        private IpV4Address _me;

        private int _socket;
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="NativeSocket"/> class.
        /// </summary>
        public NativeSocket()
        {
            _socket = CreateNative();
            StartupNative();
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="NativeSocket"/> class.
        /// </summary>
        ~NativeSocket()
        {
            Dispose(false);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
        }

        /// <inheritdoc />
        public IpV4Address GetLocalIp() => _me;

        /// <inheritdoc />
        public int ReceiveFrom(ref IpV4Address address, byte[] buffer, int length)
        {
            return ReceiveNative(_socket, ref address, buffer, length);
        }

        /// <inheritdoc />
        public int Send(ref IpV4Address address, byte[] buffer, int length)
        {
            return SendNative(_socket, ref address, buffer, length);
        }

        /// <inheritdoc />
        public int Bind(ref IpV4Address address)
        {
            _me = new IpV4Address(address.Address, address.Port);

            return BindNative(_socket, ref address);
        }

        /// <inheritdoc />
        public int Poll(long timeout)
        {
            return PollNative(_socket, timeout);
        }

        /// <inheritdoc />
        public int SetNonBlocking()
        {
            return SetNonBlockingNative(_socket);
        }

        /// <inheritdoc />
        public void Close()
        {
            CloseNative(ref _socket);
        }

        [DllImport(LibName, EntryPoint="udp_toolkit_send", CallingConvention = CallingConvention.Cdecl)]
        private static extern int SendNative(int socket, ref IpV4Address address, byte[] buffer, int length);

        [DllImport(LibName, EntryPoint="udp_toolkit_startup", CallingConvention = CallingConvention.Cdecl)]
        private static extern int StartupNative();

        [DllImport(LibName, EntryPoint="udp_toolkit_cleanup", CallingConvention = CallingConvention.Cdecl)]
        private static extern int CleanupNative();

        [DllImport(LibName, EntryPoint="udp_toolkit_create", CallingConvention = CallingConvention.Cdecl)]
        private static extern int CreateNative();

        [DllImport(LibName, EntryPoint="udp_toolkit_print_ip", CallingConvention = CallingConvention.Cdecl)]
        private static extern void PrintIpNative(ref IpV4Address address);

        [DllImport(LibName, EntryPoint = "udp_toolkit_receive", CallingConvention = CallingConvention.Cdecl)]
        private static extern int ReceiveNative(int socket, ref IpV4Address address, byte[] buffer, int length);

        [DllImport(LibName, EntryPoint = "udp_toolkit_bind", CallingConvention = CallingConvention.Cdecl)]
        private static extern int BindNative(int socket, ref IpV4Address address);

        [DllImport(LibName, EntryPoint = "udp_toolkit_poll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int PollNative(int socket, long timeout);

        [DllImport(LibName, EntryPoint = "udp_toolkit_set_nonblocking", CallingConvention = CallingConvention.Cdecl)]
        private static extern int SetNonBlockingNative(int socket);

        [DllImport(LibName, EntryPoint = "udp_toolkit_close", CallingConvention = CallingConvention.Cdecl)]
        private static extern int CloseNative(ref int socket);

        private void Dispose(
            bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                Close();
                CleanupNative();
            }

            _disposed = true;
        }
    }
}