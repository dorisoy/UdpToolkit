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

        void PublishCore<TResponse>(DataGram<TResponse> dataGram, UdpMode udpMode, Func<TResponse, byte[]> serializer);
    }
}
