namespace UdpToolkit.Jobs
{
    using System;
    using System.Threading.Tasks;
    using UdpToolkit.Contexts;
    using UdpToolkit.Core;
    using UdpToolkit.Logging;
    using UdpToolkit.Network;
    using UdpToolkit.Network.Channels;
    using UdpToolkit.Serialization;

    public sealed class WorkerJob
    {
        private readonly IUdpToolkitLogger _udpToolkitLogger;
        private readonly ISerializer _serializer;
        private readonly ISubscriptionManager _subscriptionManager;
        private readonly IRoomManager _roomManager;
        private readonly IBroadcaster _broadcaster;

        public WorkerJob(
            ISubscriptionManager subscriptionManager,
            IRoomManager roomManager,
            ISerializer serializer,
            IUdpToolkitLogger udpToolkitLogger,
            IBroadcaster broadcaster)
        {
            _subscriptionManager = subscriptionManager;
            _serializer = serializer;
            _udpToolkitLogger = udpToolkitLogger;
            _broadcaster = broadcaster;
            _roomManager = roomManager;
        }

        public static event Action<bool> OnConnectionChanged;

        public Task Execute(
            InContext inContext)
        {
            switch (inContext.InPacket.PacketType)
            {
                case PacketType.FromServer:
                case PacketType.FromClient:
                    HandleUserDefinedEvent(ref inContext);
                    break;
                case PacketType.Protocol:
                    HandleProtocolEvent(ref inContext);
                    break;
                case PacketType.Ack:
                    if (inContext.InPacket.IsProtocolEvent)
                    {
                        HandleProtocolAck(ref inContext);
                    }
                    else
                    {
                        HandleUserDefinedAck(ref inContext);
                    }

                    break;
            }

            return Task.CompletedTask;
        }

        private void HandleProtocolEvent(
            ref InContext inContext)
        {
            var inPacket = inContext.InPacket;
            var protocolHookId = (ProtocolHookId)inPacket.HookId;

            var userDefinedSubscription = _subscriptionManager
                .GetSubscription((byte)protocolHookId);

            switch (protocolHookId)
            {
                case ProtocolHookId.Heartbeat:
                case ProtocolHookId.P2P:
                case ProtocolHookId.Disconnect:
                case ProtocolHookId.Connect:

                    userDefinedSubscription?.OnProtocolEvent?.Invoke(
                        inPacket.Serializer(),
                        inPacket.ConnectionId,
                        _serializer);

                    break;
            }
        }

        private void HandleUserDefinedEvent(
            ref InContext inContext)
        {
            var inPacket = inContext.InPacket;
            var userDefinedSubscription = _subscriptionManager
                .GetSubscription(inPacket.HookId);

            if (userDefinedSubscription == null)
            {
                _udpToolkitLogger.Error($"Subscription with id {inPacket.HookId} not found! {nameof(HandleUserDefinedEvent)}");

                return;
            }

            var roomId = userDefinedSubscription.OnEvent(
                    inPacket.Serializer(),
                    inPacket.ConnectionId,
                    _serializer,
                    _roomManager);

            if (inPacket.PacketType == PacketType.FromClient)
            {
                _broadcaster.Broadcast(
                    packetType: PacketType.FromServer,
                    serializer: inPacket.Serializer,
                    caller: inPacket.ConnectionId,
                    roomId: roomId,
                    hookId: inPacket.HookId,
                    channelType: inPacket.ChannelType,
                    broadcastMode: userDefinedSubscription.BroadcastMode);
            }
        }

        private void HandleUserDefinedAck(
            ref InContext inContext)
        {
            var inPacket = inContext.InPacket;
            var userDefinedSubscription = _subscriptionManager
                .GetSubscription(inPacket.HookId);

            userDefinedSubscription?.OnAck?.Invoke(inPacket.ConnectionId);
        }

        private void HandleProtocolAck(
            ref InContext inContext)
        {
            var inPacket = inContext.InPacket;
            var protocolHookId = (ProtocolHookId)inPacket.HookId;
            var userDefinedSubscription = _subscriptionManager
                .GetSubscription((byte)protocolHookId);

            switch (protocolHookId)
            {
                case ProtocolHookId.Heartbeat:
                case ProtocolHookId.P2P:
                case ProtocolHookId.Disconnect:
                case ProtocolHookId.Connect:
                    switch (protocolHookId)
                    {
                        case ProtocolHookId.Connect:
                        case ProtocolHookId.Disconnect:
                            OnConnectionChanged?.Invoke(protocolHookId == ProtocolHookId.Connect);
                            break;
                    }

                    userDefinedSubscription?.OnAck?.Invoke(inPacket.ConnectionId);
                    break;
            }
        }
    }
}