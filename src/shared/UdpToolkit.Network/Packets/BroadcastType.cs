namespace UdpToolkit.Network.Packets
{
    public enum BroadcastType : byte
    {
        Caller = 0,
        Room = 1,
        Server = 2,
        RoomExceptCaller = 3,
        AckToServer = 4,
    }
}