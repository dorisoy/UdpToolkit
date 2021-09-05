// ReSharper disable once CheckNamespace
namespace UdpToolkit.Framework.Contracts
{
    /// <summary>
    /// Broadcast mode.
    /// </summary>
    public enum BroadcastMode : byte
    {
        /// <summary>
        /// The event will be processed by callback without broadcasting.
        /// </summary>
        None = 0,

        /// <summary>
        /// The event will be processed by callback with broadcasting to the caller.
        /// </summary>
        Caller = 1,

        /// <summary>
        /// The event will be processed by callback with broadcasting to all room clients.
        /// </summary>
        Room = 2,

        /// <summary>
        /// The event will be processed by callback with broadcasting to all server clients (not implemented).
        /// </summary>
        Server = 3,

        /// <summary>
        /// The event will be processed by callback with broadcasting to all room clients except the caller.
        /// </summary>
        RoomExceptCaller = 4,
    }
}