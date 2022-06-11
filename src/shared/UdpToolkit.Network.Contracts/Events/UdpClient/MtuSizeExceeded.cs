namespace UdpToolkit.Network.Contracts.Events.UdpClient
{
    /// <summary>
    /// Raised when MTU size for UDP packet exceeded.
    /// </summary>
    public readonly struct MtuSizeExceeded
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MtuSizeExceeded"/> struct.
        /// </summary>
        /// <param name="mtuLimit">MTU limit.</param>
        /// <param name="actualMtuSize">Actual MTU size.</param>
        public MtuSizeExceeded(
            int mtuLimit,
            int actualMtuSize)
        {
            MtuLimit = mtuLimit;
            ActualMtuSize = actualMtuSize;
        }

        /// <summary>
        /// Gets MTU limit value.
        /// </summary>
        public int MtuLimit { get; }

        /// <summary>
        /// Gets actual MTU size value.
        /// </summary>
        public int ActualMtuSize { get; }
    }
}