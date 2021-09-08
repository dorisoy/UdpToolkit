namespace UdpToolkit.Logging.Serilog
{
    using global::Serilog;
    using global::Serilog.Events;

    public sealed class SerilogLogger : global::UdpToolkit.Logging.ILogger
    {
        private readonly ILogger _logger;

        public SerilogLogger(
            ILogger logger)
        {
            _logger = logger;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return _logger.IsEnabled(Map(logLevel));
        }

        public void Warning(
            string message)
        {
            _logger.Warning("{@message}", message);
        }

        public void Error(
            string message)
        {
            _logger.Error("{@message}", message);
        }

        public void Information(
            string message)
        {
            _logger.Information("{@message}", message);
        }

        public void Debug(
            string message)
        {
            _logger.Debug("{@message}", message);
        }

        private LogEventLevel Map(LogLevel logLevel)
        {
            switch (logLevel)
            {
                case LogLevel.Debug:
                    return LogEventLevel.Debug;
                case LogLevel.Information:
                    return LogEventLevel.Information;
                case LogLevel.Warning:
                    return LogEventLevel.Warning;
                case LogLevel.Error:
                    return LogEventLevel.Error;
                default:
                    return LogEventLevel.Debug;
            }
        }
    }
}