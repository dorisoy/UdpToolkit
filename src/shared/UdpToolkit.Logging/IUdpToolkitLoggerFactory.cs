namespace UdpToolkit.Logging
{
    public interface IUdpToolkitLoggerFactory
    {
        IUdpToolkitLogger Create<TScope>();
    }
}