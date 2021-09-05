namespace UdpToolkit.Network.Contracts.Sockets
{
    using System;

    /// <summary>
    /// Socket abstraction for different socket types support.
    /// </summary>
    public interface ISocket : IDisposable
    {
        /// <summary>
        /// Get own socket ip address.
        /// </summary>
        /// <returns>Ip address.</returns>
        IpV4Address GetLocalIp();

        /// <summary>
        /// Async Receive data from socket.
        /// </summary>
        /// <param name="address">Remote ip.</param>
        /// <param name="buffer">Buffer for udp packet.</param>
        /// <param name="length">Buffer length.</param>
        /// <returns>Received bytes count.</returns>
        int ReceiveFrom(ref IpV4Address address, byte[] buffer, int length);

        /// <summary>
        /// Send data to destination.
        /// </summary>
        /// <param name="address">Destination Ip address.</param>
        /// <param name="buffer">Buffer for udp packet.</param>
        /// <param name="length">Buffer length.</param>
        /// <returns>
        /// 0 or any positive integer - count of sent characters
        /// -1 - on error.
        /// </returns>
        int Send(ref IpV4Address address, byte[] buffer, int length);

        /// <summary>
        /// Bind socket to ip address.
        /// </summary>
        /// <param name="address">Ip address.</param>
        /// <returns>
        /// -1 - on error
        /// 0 - on success.
        /// </returns>
        int Bind(ref IpV4Address address);

        /// <summary>
        /// Poll received data in socket.
        /// </summary>
        /// <param name="timeout">Poll timeout.</param>
        /// <returns>
        /// -1 - on poll error
        /// 0 - on poll timeout
        /// int > 0 - count of ready descriptors.
        /// </returns>
        int Poll(long timeout);

        /// <summary>
        /// Set non blocking option for socket.
        /// </summary>
        /// <returns>
        /// -1 - on error
        /// 1 - on success.
        /// </returns>
        int SetNonBlocking();

        /// <summary>
        /// Close socket.
        /// </summary>
        void Close();
    }
}