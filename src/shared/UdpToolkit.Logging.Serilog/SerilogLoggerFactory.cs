namespace UdpToolkit.Logging.Serilog
{
    using global::Serilog;

    public sealed class SerilogLoggerFactory : IUdpToolkitLoggerFactory
    {
        public IUdpToolkitLogger Create<TScope>()
        {
            return new SerilogLogger(logger: Log.Logger.ForContext<TScope>());
        }
    }
}