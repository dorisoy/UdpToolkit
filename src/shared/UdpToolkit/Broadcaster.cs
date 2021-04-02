namespace UdpToolkit
{
    using System;
    using UdpToolkit.Contexts;
    using UdpToolkit.Core;
    using UdpToolkit.Network;
    using UdpToolkit.Network.Channels;
    using UdpToolkit.Network.Packets;
    using UdpToolkit.Network.Queues;

    public sealed class Broadcaster : IBroadcaster
    {
        private readonly HostSettings _hostSettings;
        private readonly IAsyncQueue<HostOutContext> _hostOutQueue;
        private readonly IAsyncQueue<ClientOutContext> _clientClientOutQueue;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IRoomManager _roomManager;
        private readonly IConnectionPool _connectionPool;
        private readonly IConnection _remoteHostConnection;

        public Broadcaster(
            IAsyncQueue<HostOutContext> hostOutQueue,
            IAsyncQueue<ClientOutContext> clientOutQueue,
            IDateTimeProvider dateTimeProvider,
            IRoomManager roomManager,
            IConnectionPool connectionPool,
            HostSettings hostSettings,
            IConnection remoteHostConnection)
        {
            _hostOutQueue = hostOutQueue;
            _dateTimeProvider = dateTimeProvider;
            _roomManager = roomManager;
            _connectionPool = connectionPool;
            _hostSettings = hostSettings;
            _clientClientOutQueue = clientOutQueue;
            _remoteHostConnection = remoteHostConnection;
        }

        public void Broadcast(
            Func<byte[]> serializer,
            Guid caller,
            int roomId,
            byte hookId,
            PacketType packetType,
            ChannelType channelType,
            BroadcastMode broadcastMode)
        {
            var utcNow = _dateTimeProvider.UtcNow();

            switch (broadcastMode)
            {
                // TODO Select queue by hashed connectionId
                case BroadcastMode.Caller:
                    _hostOutQueue.Produce(new HostOutContext(
                        resendTimeout: _hostSettings.ResendPacketsTimeout,
                        createdAt: utcNow,
                        roomId: roomId,
                        broadcastMode: broadcastMode,
                        outPacket: new OutPacket(
                            hookId: hookId,
                            channelType: channelType,
                            packetType: packetType,
                            connectionId: caller,
                            serializer: serializer,
                            createdAt: utcNow,
                            ipEndPoint: _connectionPool.TryGetConnection(caller).GetIp())));
                    return;
                case BroadcastMode.AllConnections:
                    _connectionPool.Apply(
                        action: (connection) =>
                        {
                            _hostOutQueue.Produce(new HostOutContext(
                                resendTimeout: _hostSettings.ResendPacketsTimeout,
                                createdAt: utcNow,
                                roomId: roomId,
                                broadcastMode: broadcastMode,
                                outPacket: new OutPacket(
                                    hookId: hookId,
                                    channelType: channelType,
                                    packetType: packetType,
                                    connectionId: caller,
                                    serializer: serializer,
                                    createdAt: utcNow,
                                    ipEndPoint: connection.GetIp())));
                        });

                    return;
                case BroadcastMode.Server:
                    return;
            }

            var room = _roomManager.GetRoom(roomId);
            switch (broadcastMode)
            {
                case BroadcastMode.RoomExceptCaller:
                    for (var i = 0; i < room.Count; i++)
                    {
                        var connection = _connectionPool.TryGetConnection(room[i]);
                        if (connection.ConnectionId == caller)
                        {
                            continue;
                        }

                        _hostOutQueue.Produce(new HostOutContext(
                            resendTimeout: _hostSettings.ResendPacketsTimeout,
                            createdAt: utcNow,
                            roomId: roomId,
                            broadcastMode: broadcastMode,
                            outPacket: new OutPacket(
                                hookId: hookId,
                                channelType: channelType,
                                packetType: packetType,
                                connectionId: room[i],
                                serializer: serializer,
                                createdAt: utcNow,
                                ipEndPoint: connection.GetIp())));
                    }

                    return;
                case BroadcastMode.Room:
                    for (var i = 0; i < room.Count; i++)
                    {
                        var ip = _connectionPool
                            .TryGetConnection(room[i])
                            .GetIp();

                        _hostOutQueue.Produce(new HostOutContext(
                            resendTimeout: _hostSettings.ResendPacketsTimeout,
                            createdAt: utcNow,
                            roomId: roomId,
                            broadcastMode: broadcastMode,
                            outPacket: new OutPacket(
                                hookId: hookId,
                                channelType: channelType,
                                packetType: packetType,
                                connectionId: room[i],
                                serializer: serializer,
                                createdAt: utcNow,
                                ipEndPoint: ip)));
                    }

                    return;

                default:
                    throw new ArgumentOutOfRangeException(nameof(broadcastMode), broadcastMode, null);
            }
        }

        public void Broadcast(
            Func<byte[]> serializer,
            Guid caller,
            byte hookId,
            PacketType packetType,
            ChannelType channelType,
            BroadcastMode broadcastMode)
        {
            // TODO Select queue by hashed connectionId
            var utcNow = _dateTimeProvider.UtcNow();
            _clientClientOutQueue.Produce(new ClientOutContext(
                resendTimeout: _hostSettings.ResendPacketsTimeout,
                createdAt: utcNow,
                broadcastMode: broadcastMode,
                outPacket: new OutPacket(
                    hookId: hookId,
                    channelType: channelType,
                    packetType: packetType,
                    connectionId: caller,
                    serializer: serializer,
                    createdAt: utcNow,
                    ipEndPoint: _remoteHostConnection.GetIp())));
        }

        public void Dispose()
        {
            _hostOutQueue?.Dispose();
        }
    }
}