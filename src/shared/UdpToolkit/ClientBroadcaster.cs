namespace UdpToolkit
{
    using System;
    using UdpToolkit.Contexts;
    using UdpToolkit.Core;
    using UdpToolkit.Network;
    using UdpToolkit.Network.Channels;
    using UdpToolkit.Network.Packets;

    public class ClientBroadcaster : IClientBroadcaster
    {
        private readonly IQueueDispatcher<ClientOutContext> _clientOutQueueDispatcher;
        private readonly IConnection _hostConnection;
        private readonly IDateTimeProvider _dateTimeProvider;

        public ClientBroadcaster(
            IQueueDispatcher<ClientOutContext> clientOutQueueDispatcher,
            IConnection hostConnection,
            IDateTimeProvider dateTimeProvider)
        {
            _clientOutQueueDispatcher = clientOutQueueDispatcher;
            _hostConnection = hostConnection;
            _dateTimeProvider = dateTimeProvider;
        }

        public void Broadcast(
            Func<byte[]> serializer,
            Guid caller,
            byte hookId,
            PacketType packetType,
            ChannelType channelType,
            BroadcastMode broadcastMode)
        {
            var utcNow = _dateTimeProvider.UtcNow();
            _clientOutQueueDispatcher
                .Dispatch(caller)
                .Produce(new ClientOutContext(
                    createdAt: utcNow,
                    broadcastMode: broadcastMode,
                    outPacket: new OutPacket(
                        hookId: hookId,
                        channelType: channelType,
                        packetType: packetType,
                        connectionId: caller,
                        serializer: serializer,
                        createdAt: utcNow,
                        ipEndPoint: _hostConnection.GetIp())));
        }

        public void Dispose()
        {
            _clientOutQueueDispatcher?.Dispose();
        }
    }
}