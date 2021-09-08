namespace UdpToolkit.Logging
{
    public interface ILoggerFactory
    {
        ILogger Create<TScope>();
    }
}