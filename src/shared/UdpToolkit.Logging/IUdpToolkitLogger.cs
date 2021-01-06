namespace UdpToolkit.Logging
{
    public interface IUdpToolkitLogger
    {
        void Warning(
            string message);

        void Error(
            string message);

        void Information(
            string message);

        void Debug(
            string message);
    }
}