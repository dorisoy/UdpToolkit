namespace UdpToolkit.Core
{
    public enum BroadcastMode : byte
    {
        Caller = 0,
        Room = 1,
        Server = 2,
        AllConnections = 3,
        RoomExceptCaller = 4,
    }
}