namespace UdpToolkit.Logging.Serilog
{
    using global::Serilog;

    public sealed class SerilogLoggerFactory : ILoggerFactory
    {
        public global::UdpToolkit.Logging.ILogger Create<TScope>()
        {
            return new SerilogLogger(logger: Log.Logger.ForContext<TScope>());
        }
    }
}