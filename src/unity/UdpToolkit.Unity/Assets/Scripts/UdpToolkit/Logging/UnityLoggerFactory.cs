namespace UdpToolkit.Logging
{
    /// <summary>
    /// Factory for creating UnityLogger instance.
    /// </summary>
    public sealed class UnityLoggerFactory : ILoggerFactory
    {
        private readonly LogLevel _logLevel;

        /// <summary>
        /// Initializes a new instance of the <see cref="UnityLoggerFactory"/> class.
        /// </summary>
        /// <param name="logLevel">Logging level.</param>
        public UnityLoggerFactory(
            LogLevel logLevel)
        {
            _logLevel = logLevel;
        }

        /// <inheritdoc/>
        public ILogger Create<TScope>()
        {
            return new UnityLogger(_logLevel);
        }
    }
}