namespace Cubes.Shared.Server
{
    using System;
    using Cubes.Shared.Events;
    using UdpToolkit.Core;
    using UdpToolkit.Framework;

    public class MoveEventHandler : IEventHandler<MoveEvent>
    {
        private readonly IHost host;
        private INetworkThreadDispatcher dispatcher;

        public MoveEventHandler(
            IHost host,
            INetworkThreadDispatcher dispatcher)
        {
            this.host = host;
            this.dispatcher = dispatcher;
            Subscribe();
        }

        public event Action<MoveEvent> OnEvent;

        public event Action<Guid> OnAck;

        public event Action<Guid> OnTimeout;

        private void Subscribe()
        {
            host
                .On<MoveEvent>(
                    onEvent: (peerId, moveEvent) =>
                    {
                        dispatcher?.Enqueue(() => OnEvent?.Invoke(moveEvent));

                        return moveEvent.RoomId;
                    },
                    onAck: (peerId) =>
                    {
                        dispatcher?.Enqueue(() => OnAck?.Invoke(peerId));
                    },
                    onTimeout: (peerId) =>
                    {
                        dispatcher?.Enqueue(() => OnTimeout?.Invoke(peerId));
                    },
                    hookId: 3,
                    broadcastMode: BroadcastMode.Room);
        }
    }
}