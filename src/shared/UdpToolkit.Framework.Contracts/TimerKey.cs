namespace UdpToolkit.Framework.Contracts
{
    public readonly struct TimerKey
    {
        public TimerKey(
            int roomId,
            string timerId)
        {
            RoomId = roomId;
            TimerId = timerId;
        }

        public int RoomId { get; }

        public string TimerId { get; }
    }
}