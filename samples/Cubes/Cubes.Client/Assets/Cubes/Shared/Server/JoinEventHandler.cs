namespace Cubes.Shared.Server
{
    using System;
    using Cubes.Shared.Events;
    using Serilog;
    using UdpToolkit;
    using UdpToolkit.Core;
    using UnityEngine;

    public class JoinEventHandler : IEventHandler<JoinEvent>
    {
        private readonly IHost host;
        private INetworkThreadDispatcher dispatcher;

        public JoinEventHandler(
            IHost host,
            INetworkThreadDispatcher dispatcher)
        {
            this.host = host;
            this.dispatcher = dispatcher;
            Subscribe();
        }

        public event Action<JoinEvent> OnEvent;

        public event Action<Guid> OnAck;

        public event Action<Guid> OnTimeout;

        private void Subscribe()
        {
            host
                .On<JoinEvent>(
                    onEvent: (peerId, joinEvent, roomManager) =>
                    {
                        roomManager
                            .JoinOrCreate(joinEvent.RoomId, peerId);

                        Log.Logger.Information($"{joinEvent.Nickname} joined to room!");

                        dispatcher?.Enqueue(() => OnEvent?.Invoke(joinEvent));

                        return joinEvent.RoomId;
                    },
                    scheduleCall: new ScheduledCall<JoinEvent>(
                        timerId: Timers.SpawnTimer,
                        action: (joinEvent) =>
                        {
                            Log.Logger.Information($"Scheduled event!");

                            host.PublishCore(
                                @event: new SpawnEvent(
                                    playerId: Guid.Empty,
                                    roomId: joinEvent.RoomId,
                                    nickname: joinEvent.Nickname,
                                    position: new Vector3(x: 5, y: 5, z: 5)),
                                roomId: joinEvent.RoomId,
                                hookId: 2,
                                udpMode: UdpMode.ReliableUdp,
                                broadcastMode: BroadcastMode.Room);
                        },
                        delay: TimeSpan.FromMilliseconds(7000)),
                    onAck: (peerId) =>
                    {
                        dispatcher?.Enqueue(() => OnAck?.Invoke(peerId));
                    },
                    onTimeout: (peerId) =>
                    {
                        dispatcher?.Enqueue(() => OnTimeout?.Invoke(peerId));
                    },
                    hookId: 1,
                    broadcastMode: BroadcastMode.Room);
        }
    }
}
