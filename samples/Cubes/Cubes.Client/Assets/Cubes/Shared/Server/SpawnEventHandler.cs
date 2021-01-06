namespace Cubes.Shared.Server
{
    using System;
    using Cubes.Shared.Events;
    using UdpToolkit;
    using UdpToolkit.Core;

    public class SpawnEventHandler : IEventHandler<SpawnEvent>
    {
        private readonly IHost host;
        private INetworkThreadDispatcher dispatcher;

        public SpawnEventHandler(
            IHost host,
            INetworkThreadDispatcher dispatcher)
        {
            this.host = host;
            this.dispatcher = dispatcher;
            Subscribe();
        }

        public event Action<SpawnEvent> OnEvent;

        public event Action<Guid> OnAck;

        public event Action<Guid> OnTimeout;

        private void Subscribe()
        {
            host
                .On<SpawnEvent>(
                    onEvent: (peerId, spawnEvent) =>
                    {
                        dispatcher?.Enqueue(() => OnEvent?.Invoke(spawnEvent));

                        return spawnEvent.RoomId;
                    },
                    onAck: (peerId) =>
                    {
                        dispatcher?.Enqueue(() => OnAck?.Invoke(peerId));
                    },
                    onTimeout: (peerId) =>
                    {
                        dispatcher?.Enqueue(() => OnTimeout?.Invoke(peerId));
                    },
                    hookId: 2,
                    broadcastMode: BroadcastMode.Room);
        }
    }
}