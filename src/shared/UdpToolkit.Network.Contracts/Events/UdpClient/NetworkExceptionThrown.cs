namespace UdpToolkit.Network.Contracts.Events.UdpClient
{
    using System;

    /// <summary>
    /// Raised when network exception thrown.
    /// </summary>
    public readonly struct NetworkExceptionThrown
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NetworkExceptionThrown"/> struct.
        /// </summary>
        /// <param name="exception">Exception instance.</param>
        public NetworkExceptionThrown(
            Exception exception)
        {
            Exception = exception;
        }

        /// <summary>
        /// Gets Exception instance.
        /// </summary>
        public Exception Exception { get; }
    }
}