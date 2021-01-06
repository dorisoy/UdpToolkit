namespace Cubes.Shared.Server
{
    using System;
    using UdpToolkit;
    using UdpToolkit.Core;
    using UdpToolkit.Core.ProtocolEvents;
    using UdpToolkit.Network.Channels;

    public sealed class ConnectEventHandler : EventHandlerBase, IEventHandler<Connect>
    {
        private readonly IHost host;
        private readonly INetworkThreadDispatcher dispatcher;

        public ConnectEventHandler(
            IHost host,
            INetworkThreadDispatcher dispatcher)
            : base(host, dispatcher)
        {
            this.host = host;
            this.dispatcher = dispatcher;
            Subscribe();
        }

        public event Action<Connect> OnEvent;

#pragma warning disable CS0067
        public event Action<Guid> OnAck;

        public event Action<Guid> OnTimeout;
#pragma warning restore CS0067

        private void Subscribe()
        {
            host
                .OnProtocol<Connect>(
                    onProtocolEvent: (peerId, connect) =>
                    {
                        dispatcher?.Enqueue(() => OnEvent?.Invoke(connect));
                    },
                    onAck: (peerId) =>
                    {
                        dispatcher?.Enqueue(() => OnAck?.Invoke(peerId));
                    },
                    onTimeout: (peerId) =>
                    {
                        dispatcher?.Enqueue(() => OnTimeout?.Invoke(peerId));
                    },
                    protocolHookId: ProtocolHookId.Connect);
        }
    }
}