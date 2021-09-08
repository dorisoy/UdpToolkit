# How it works

UdpToolkit consists of two main parts:
- Network for working with bytes, IP addresses, custom network headers e.t.c. 
- Framework for working with logic of your game like `Move`, `Shot`, `Heal`, `Join` events e.t.c.   

## Network

### Network protocol

`|protocol header|payload|`

### Protocol header

| Number  | Name         | Type        | Size in bytes | Description                                                                                            |
| ------- | ------------ | ----------- | ------------- |------------------------------------------------------------------------------------------------------- |
| 1.      | ChannelId    | byte        | 1             | Identifier of channel.                                                                                 |
| 2.      | Id           | ushort      | 2             | Identifier of current packet.                                                                          |
| 3.      | Acks         | uint        | 4             | 32 bit with info about previous packets relative current. 1bit per packet. Reserved, not implemented.  |
| 4.      | ConnectionId | Guid        | 16            | Identifier of connection.                                                                              |
| 5.      | PacketType   | PacketType  | 1             | Packet type.                                                                                           |

### Internal protocol

| Type               | Size in bytes | Description                                     | Behaviour                                                                                     |
| ------------------ | ------------- |------------------------------------------------ | --------------------------------------------------------------------------------------------- |
| Connect            | 24            | Incoming connection packet.                     | Send by the client, to initiate the connection.                                               |
| Connect OR Ack     | 24            | Acknowledge for connection packet.              | Send by the server if connection is established.                                              |
| Disconnect         | 24            | Incoming disconnect packet.                     | Send by the client, to explicit disconnection.                                                |                                                             
| Disconnect OR Ack  | 24            | Acknowledge for disconnect packet.              | Send by the server if client disconnected.                                                    |
| Heartbeat          | 24            | Incoming heartbeat packet.                      | Send by client for two reasons: 1) RTT measurement, 2) Initiate resending of pending packets. |
| Heartbeat OR Ack   | 24            | Acknowledge for heartbeat packet.               | Send by the server if heartbeat packet received.                                              |
| UserDefined        | 24            | Incoming user-defined packet.                   | Send by client.                                                                               |
| UserDefined OR Ack | 24            | Acknowledge for user-defined packet.            | Send by server if user-defined packet received.                                               |
| Ack                | 24            | Acknowledge bit, could be added to any packets. | Added for all incoming reliable packets.                                                      |

### Expansion points

`ISocket` - For performance reasons default sockets in `UdpToolkit` are written in C, but very difficult to support this solution for all possible platforms. Fallback on managed version always available.

`IChannel` - Unfortunately, no silver bullet for reliable UDP protocols if you want to implement your own mechanism for working with the `Protocol header` you could implement a custom channel. All actions in the scope of the channel are thread-safe.

`IConnectionIdFactory` - By default `ConnectionId` is generated randomly if you want to change this behavior just pass the alternative implementation.
## Framework

### Framework protocol

`|framework header|payload|`

### Protocol header (Explicit)

Consist only of 1 byte per user-defined event. This byte is assigned automatically when code-generation for user-defined types run. You could find all generated values in the `HostWorkerGenerated.cs` file.
Required for matching user-defined types with serialization actions.

### Protocol header (Implicit)

The client should include `RoomId:Guid` (16 bytes) in each event. This is required to restrict broadcasting scope on the framework side.

### Concurrency

All actions performed in `onEvent` callback are not thread-safe. This is absolutely the same behavior as in popular web frameworks for .net like `asp.net core` or `GRPC`.

But each connection deterministically dispatches into the same thread and all events from this connection are processed by the same thread.
```c#
host.On<Message>(
    onEvent: (connectionId, ip, message, roomManager) =>
    {
        // not thread-safe here
        Console.WriteLine($"Message received: {message.Text}");
        roomManager.JoinOrCreate(1, connectionId, ip);
        return message.RoomId;
    });
```

For thread-safe events processing in-room scope, you could use other libraries or implement locking (named lock) the room by yourself.