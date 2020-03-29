namespace UdpToolkit.Tests.Fakes
{
    using System;
    using UdpToolkit.Core;
    using UdpToolkit.Utils;

    public class FakeDateTimeProvider : IDateTimeProvider
    {
        private string _date;

        public FakeDateTimeProvider(string date)
        {
            _date = date;
        }

        public void RewindDateTime(string date)
        {
            _date = date;
        }

        public DateTimeOffset UtcNow()
        {
            return DateTimeOffset.Parse(_date);
        }
    }
}