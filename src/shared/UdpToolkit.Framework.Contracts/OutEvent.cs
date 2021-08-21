namespace UdpToolkit.Framework.Contracts
{
    public readonly struct OutEvent<TEvent>
    {
        public OutEvent(
            int roomId,
            TEvent @event,
            int delayInMs,
            BroadcastMode broadcastMode,
            byte channelId)
        {
            RoomId = roomId;
            Event = @event;
            DelayInMs = delayInMs;
            BroadcastMode = broadcastMode;
            ChannelId = channelId;
        }

        public int RoomId { get; }

        public TEvent Event { get; }

        public int DelayInMs { get; }

        public BroadcastMode BroadcastMode { get; }

        public byte ChannelId { get; }
    }
}