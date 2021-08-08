namespace UdpToolkit.Framework.Contracts
{
    public readonly struct TimerKey
    {
        public TimerKey(
            int roomId,
            short timerId)
        {
            RoomId = roomId;
            TimerId = timerId;
        }

        public int RoomId { get; }

        public short TimerId { get; }
    }
}