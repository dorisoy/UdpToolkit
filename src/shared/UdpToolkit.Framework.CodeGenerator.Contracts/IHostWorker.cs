// ReSharper disable once CheckNamespace
namespace UdpToolkit.Framework.Contracts
{
    using System;
    using UdpToolkit.Logging;
    using UdpToolkit.Serialization;

    public interface IHostWorker : IDisposable
    {
        IUdpToolkitLogger Logger { get; set; }

        IRoomManager RoomManager { get; set; }

        IScheduler Scheduler { get; set; }

        ISerializer Serializer { get; set; }

        public void Process(
            InPacket inPacket);

        public byte[] Process(
            OutPacket outPacket);
    }
}