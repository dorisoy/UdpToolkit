namespace UdpToolkit.Logging
{
    public interface ILogger
    {
        public bool IsEnabled(
            LogLevel logLevel);

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