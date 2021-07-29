namespace UdpToolkit.Core
{
    public enum BroadcastMode : byte
    {
        None = 0,
        Caller = 1,
        Room = 2,
        Server = 3,
        AllConnections = 4,
        RoomExceptCaller = 5,
    }
}