namespace UdpToolkit.Core
{
    using System;
    using System.Threading.Tasks;
    using UdpToolkit.Serialization;

    public interface IHost : IDisposable
    {
        IServerHostClient ServerHostClient { get; }

        Task RunAsync();

        void Stop();

        void OnCore<TEvent>(Subscription subscription, byte hookId);

        void PublishCore<TEvent>(Func<IDatagramBuilder, Datagram<TEvent>> datagramFactory, UdpMode udpMode);

        void PublishInternal<TEvent>(Datagram<TEvent> datagram, UdpMode udpMode, Func<TEvent, byte[]> serializer);
    }
}
