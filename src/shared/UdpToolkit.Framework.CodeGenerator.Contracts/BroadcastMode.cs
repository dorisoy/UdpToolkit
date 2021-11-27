// ReSharper disable once CheckNamespace
namespace UdpToolkit.Framework.Contracts
{
    /// <summary>
    /// Broadcast mode.
    /// </summary>
    public enum BroadcastMode : byte
    {
        /// <summary>
        /// The event will be processed by subscription without broadcasting.
        /// </summary>
        None = 0,

        /// <summary>
        /// The event will be processed by subscription with broadcasting to the caller.
        /// </summary>
        Caller = 1,

        /// <summary>
        /// The event will be processed by subscription with broadcasting to all group clients.
        /// </summary>
        Group = 2,

        /// <summary>
        /// The event will be processed by subscription with broadcasting to all server clients (not implemented).
        /// </summary>
        Server = 3,

        /// <summary>
        /// The event will be processed by subscription with broadcasting to all group clients except the caller.
        /// </summary>
        GroupExceptCaller = 4,
    }
}