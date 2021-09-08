namespace UdpToolkit.Logging
{
    public class SimpleConsoleLoggerFactory : ILoggerFactory
    {
        private readonly LogLevel _logLevel;

        public SimpleConsoleLoggerFactory(LogLevel logLevel)
        {
            _logLevel = logLevel;
        }

        public ILogger Create<TScope>()
        {
            return new SimpleConsoleLogger(_logLevel);
        }
    }
}