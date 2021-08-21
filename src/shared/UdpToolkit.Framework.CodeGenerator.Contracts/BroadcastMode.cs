// ReSharper disable once CheckNamespace
namespace UdpToolkit.Framework.Contracts
{
    public enum BroadcastMode : byte
    {
        None = 0,
        Caller = 1,
        Room = 2,
        Server = 3,
        RoomExceptCaller = 4,
    }
}