namespace UdpToolkit.Network.Contracts.Clients
{
    using UdpToolkit.Network.Contracts.Sockets;

    /// <summary>
    /// Abstraction for creating lightweight UDP clients.
    /// </summary>
    public interface IUdpClientFactory
    {
        /// <summary>
        /// Create UdpClient.
        /// </summary>
        /// <param name="ipV4Address">Ip address for UdpClient.</param>
        /// <returns>
        /// Instance of UdpClient.
        /// </returns>
        /// <remarks>
        /// All UdpClient instances share common connection pool.
        /// </remarks>
        IUdpClient Create(
            IpV4Address ipV4Address);
    }
}