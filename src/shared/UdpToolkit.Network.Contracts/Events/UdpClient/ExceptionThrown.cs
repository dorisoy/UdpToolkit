namespace UdpToolkit.Network.Contracts.Events.UdpClient
{
    using System;

    /// <summary>
    /// Raised when network exception thrown.
    /// </summary>
    public readonly struct ExceptionThrown
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionThrown"/> struct.
        /// </summary>
        /// <param name="exception">Exception instance.</param>
        public ExceptionThrown(
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