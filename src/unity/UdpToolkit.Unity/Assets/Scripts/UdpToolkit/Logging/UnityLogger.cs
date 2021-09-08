namespace UdpToolkit.Logging
{
    /// <summary>
    /// Unity logger implementation.
    /// </summary>
    public sealed class UnityLogger : ILogger
    {
        private readonly LogLevel _logLevel;

        /// <summary>
        /// Initializes a new instance of the <see cref="UnityLogger"/> class.
        /// </summary>
        /// <param name="logLevel">Logging level.</param>
        public UnityLogger(
            LogLevel logLevel)
        {
            _logLevel = logLevel;
        }

        /// <inheritdoc/>
        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel >= _logLevel;
        }

        /// <inheritdoc/>
        public void Warning(string message)
        {
            UnityEngine.Debug.LogWarning(message);
        }

        /// <inheritdoc/>
        public void Error(string message)
        {
            UnityEngine.Debug.LogError(message);
        }

        /// <inheritdoc/>
        public void Information(string message)
        {
            UnityEngine.Debug.Log(message);
        }

        /// <inheritdoc/>
        public void Debug(string message)
        {
            UnityEngine.Debug.Log(message);
        }
    }
}
