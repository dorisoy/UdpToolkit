namespace UdpToolkit.Framework.Contracts.Events
{
    using System;

    /// <summary>
    /// Raised when exception thrown.
    /// </summary>
    public readonly struct ExceptionThrown
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UdpToolkit.Framework.Contracts.Events.ExceptionThrown"/> struct.
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