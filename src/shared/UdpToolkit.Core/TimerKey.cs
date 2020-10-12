namespace UdpToolkit.Core
{
    public readonly struct TimerKey
    {
        public TimerKey(
            ushort roomId,
            short timerId)
        {
            RoomId = roomId;
            TimerId = timerId;
        }

        public ushort RoomId { get; }

        public short TimerId { get; }
    }
}