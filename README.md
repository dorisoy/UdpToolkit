# UdpToolkit

`UdpToolkit` - library for building simple client/server applications communicated over UDP.
Library implements own expandable mechanism for sending reliable UDP datagrams.

## Disclaimer

This project still under development and I see several stages for him:

- preview (current state)
- alpha
- beta
- production-ready release

## Features

- Small and simple
- Fully asynchronous via callbacks not freeze UI thread
- Unity support via `*.unitypackage`
- Dependency free
- Channeling
- P2P support
- Memory pool for datagrams
- Serialization extension point
- Logging extension point
- netstandard2.0 support 

## Getting started
 
1) Install NuGet packages on both applications server/client:

`dotnet add package UdpToolkit --version <VERSION>` - main package for build udp server  
`dotnet add package UdpToolkit.Serialization.MsgPack --version <VERSION>` - integration package for serialization  
`dotnet add package UdpToolkit.Logging.Serilog --version <VERSION>` - integration package for logging

2) Define a server: 

```
public static class Program
{
    public static async Task Main()
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Is(LogEventLevel.Debug)
            .WriteTo.Console()
            .CreateLogger();

        var host = BuildHost();

        host.On<JoinEvent>(
            onEvent: (peerId, joinEvent, roomManager) =>
            {
                Log.Logger.Information($"{joinEvent.Nickname} joined to room!");

                return joinEvent.RoomId;
            },
            broadcastMode: BroadcastMode.Room,
            hookId: 0);

        await host
            .RunAsync()
            .ConfigureAwait(false);
    }

    private static IHost BuildHost() =>
        UdpHost
            .CreateHostBuilder()
            .ConfigureHost(settings =>
            {
                settings.Serializer = new Serializer();
                settings.InputPorts = new[] { 7000 };
                settings.OutputPorts = new[] { 8000 };
            })
            .Build();
}
```

3) Define a client:

```
public static class Program
{
    public static void Main()
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Is(LogEventLevel.Debug)
            .WriteTo.Console()
            .CreateLogger();

        var host = BuildHost();
        var client = host.ServerHostClient;
        var nickname = "keygen";

        host.On<JoinEvent>(
            onEvent: (peerId, joinEvent) =>
            {
                Log.Logger.Information($"{joinEvent.Nickname} joined to room!");
                return joinEvent.RoomId;
            },
            onAck: (peerId) =>
            {
                Log.Logger.Information($"{nickname} joined to room!");
            },
            broadcastMode: BroadcastMode.Room,
            hookId: 0);

        Task.Run(() => host.RunAsync());

        var isConnected = client
            .Connect();

        client.Publish(
            @event: new JoinEvent(roomId: 11, nickname: nickname),
            hookId: 0,
            udpMode: UdpMode.ReliableUdp);

        Console.WriteLine($"IsConnected - {isConnected}");

        Console.WriteLine("Press any key...");
        Console.ReadLine();
    }

    private static IHost BuildHost()
    {
        return UdpHost
            .CreateHostBuilder()
            .ConfigureHost((settings) =>
            {
                settings.Serializer = new Serializer();
            })
            .ConfigureServerHostClient((settings) =>
            {
                settings.ServerInputPorts = new[] { 7000 };
            })
            .Build();
    }
}
```

4) Run both applications...

5) For more details please see the `samples` folder in the project repository.

## Contributing

Feel free to open an issue if you find a bug or have any questions...