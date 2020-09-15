namespace UdpToolkit.Core
{
    using System;

    public interface IDatagramBuilder
    {
        Datagram<TEvent> ToServer<TEvent>(TEvent @event, byte hookId);

        Datagram<TEvent> All<TEvent>(TEvent @event, byte hookId);

        Datagram<TEvent> AllExcept<TEvent>(TEvent @event, Guid peerId, byte hookId);

        Datagram<TEvent> Room<TEvent>(TEvent @event, byte roomId, byte hookId);

        Datagram<TEvent> RoomExcept<TEvent>(TEvent @event, byte roomId, Guid peerId, byte hookId);

        Datagram<TEvent> Caller<TEvent>(TEvent @event, byte roomId, Guid peerId, byte hookId);

        Datagram<TEvent> Caller<TEvent>(TEvent @event, Guid peerId, byte hookId);
    }
}