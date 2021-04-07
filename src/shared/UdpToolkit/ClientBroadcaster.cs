namespace UdpToolkit
{
    using System;
    using UdpToolkit.Core;
    using UdpToolkit.Network;
    using UdpToolkit.Network.Channels;
    using UdpToolkit.Network.Packets;

    public class ClientBroadcaster : IClientBroadcaster
    {
        private readonly IQueueDispatcher<OutPacket> _outQueueDispatcher;
        private readonly IConnection _clientConnection;
        private readonly IDateTimeProvider _dateTimeProvider;

        public ClientBroadcaster(
            IQueueDispatcher<OutPacket> outQueueDispatcher,
            IConnection clientConnection,
            IDateTimeProvider dateTimeProvider)
        {
            _outQueueDispatcher = outQueueDispatcher;
            _clientConnection = clientConnection;
            _dateTimeProvider = dateTimeProvider;
        }

        public void Broadcast(
            Func<byte[]> serializer,
            Guid caller,
            byte hookId,
            PacketType packetType,
            ChannelType channelType)
        {
            var utcNow = _dateTimeProvider.UtcNow();
            _outQueueDispatcher
                .Dispatch(caller)
                .Produce(new OutPacket(
                    hookId: hookId,
                    channelType: channelType,
                    packetType: packetType,
                    connectionId: caller,
                    serializer: serializer,
                    createdAt: utcNow,
                    ipEndPoint: _clientConnection.Ip));
        }

        public void Dispose()
        {
            _outQueueDispatcher?.Dispose();
        }
    }
}