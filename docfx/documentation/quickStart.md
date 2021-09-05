# Quick start guide

## Required tools

- Installed [net5.0](https://dotnet.microsoft.com/download/dotnet/5.0)

## Project structure
1) Create class library:
`dotnet new classlib --name Contracts`
2) Create Server:
`dotnet new console --name Server`
3) Create Client:
`dotnet new console --name Client`
4) Create solution file and add all projects to them:
`dotnet new sln --name SampleProject`

The final project structure should look like this:
```
Server/Server.csproj
Client/Client.cspproj
Contracts/Contracts.cspproj
SampleProject.sln
```

## Contracts
1) Update `Contracts.cspproj` file by the following code:  

```html
<Project Sdk="Microsoft.NET.Sdk">

    <PropertyGroup>
        <TargetFramework>net5.0</TargetFramework>
        <EmitCompilerGeneratedFiles>true</EmitCompilerGeneratedFiles>
        <CompilerGeneratedFilesOutputPath>$(BaseIntermediateOutputPath)GeneratedFiles</CompilerGeneratedFilesOutputPath>
    </PropertyGroup>

    <ItemGroup>
        <PackageReference Include="UdpToolkit.Serialization.MsgPack" Version="1.0.0-preview.1.1114556675" />
        <PackageReference Include="UdpToolkit.CodeGenerator" Version="1.0.0-preview.1.1114556675" PrivateAssets="all" />
    </ItemGroup>

</Project>

```

2) Add class `Message` to library:

```
namespace Contracts
{
    using MessagePack;
    using UdpToolkit.Annotations;

    [MessagePackObject]
    [UdpEvent]
    public class Message
    {
        [Key(0)]
        public string Text { get; set; }
    }
}
```

## Server
1) Update `Server.cspproj` by the following code:  
```html
<Project Sdk="Microsoft.NET.Sdk">

    <PropertyGroup>
        <OutputType>Exe</OutputType>
        <TargetFramework>net5.0</TargetFramework>
    </PropertyGroup>

    <ItemGroup>
        <PackageReference Include="Serilog.Sinks.Console" Version="4.0.0" />
        <PackageReference Include="UdpToolkit" Version="1.0.0-preview.1.1114556675" />
        <PackageReference Include="UdpToolkit.Logging.Serilog" Version="1.0.0-preview.1.1114556675" />
    </ItemGroup>

    <ItemGroup>
        <ProjectReference Include="../Contracts/Contracts.csproj" />
    </ItemGroup>


</Project>
```

2) Add code to `Main` method: 
```c#
namespace Server
{
    using System;
    using Contracts;
    using Serilog;
    using Serilog.Events;
    using UdpToolkit;
    using UdpToolkit.Framework;
    using UdpToolkit.Framework.Contracts;
    using UdpToolkit.Framework.Contracts.Settings;
    using UdpToolkit.Logging.Serilog;
    using UdpToolkit.Serialization.MsgPack;

    public static class Program
    {
        public static void Main(string[] args)
        {
            // 1) Setup logging level:
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Is(LogEventLevel.Debug)
                .WriteTo.Console()
                .CreateLogger();

            // 2) Create UDP host:
            var host = BuildHost();

            // 3) Subscribe on event:
            host.On<Message>(
                onEvent: (connectionId, ip, message, roomManager) =>
                {
                    Log.Logger.Debug($"Message received: {message.Text}");
                    roomManager.JoinOrCreate(1, connectionId, ip);
                    return 1;
                });

            // 4) Run host:
            host.Run();

            Console.WriteLine("Press any key..");
            Console.ReadLine();
        }

        private static IHost BuildHost()
        {
            var hostSettings = new HostSettings(
                serializer: new Serializer(),
                loggerFactory: new SerilogLoggerFactory());

            return UdpHost
                .CreateHostBuilder()
                .ConfigureHost(hostSettings, settings =>
                {
                    settings.HostPorts = new[] { 7000 };
                })
                .ConfigureNetwork((settings) =>
                {
                    settings.AllowIncomingConnections = true;
                })
                .BootstrapWorker(new HostWorkerGenerated())
                .Build();
        }
    }
}
```

## Client

1) Update `Client.cspproj` file by the following code:
```html
<Project Sdk="Microsoft.NET.Sdk">

    <PropertyGroup>
        <OutputType>Exe</OutputType>
        <TargetFramework>net5.0</TargetFramework>
    </PropertyGroup>

    <ItemGroup>
        <PackageReference Include="Serilog.Sinks.Console" Version="4.0.0" />
        <PackageReference Include="UdpToolkit" Version="1.0.0-preview.1.1114556675" />
        <PackageReference Include="UdpToolkit.Logging.Serilog" Version="1.0.0-preview.1.1114556675" />
    </ItemGroup>

    <ItemGroup>
        <ProjectReference Include="../Contracts/Contracts.csproj" />
    </ItemGroup>

</Project>

```

2) Add code to `Main` method: 

```c#
namespace Client
{
    using System;
    using System.Threading;
    using Contracts;
    using Serilog;
    using Serilog.Events;
    using UdpToolkit;
    using UdpToolkit.Framework;
    using UdpToolkit.Framework.Contracts;
    using UdpToolkit.Framework.Contracts.Settings;
    using UdpToolkit.Logging.Serilog;
    using UdpToolkit.Network.Channels;
    using UdpToolkit.Serialization.MsgPack;

    public static class Program
    {
        private static bool isConnected = false;

        public static void Main(string[] args)
        {
            // 1) Setup logging level:
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Is(LogEventLevel.Debug)
                .WriteTo.Console()
                .CreateLogger();

            // 2) Create UDP host:
            var host = BuildHost();

            host.HostClient.OnConnected += (address, connectionId) =>
            {
                isConnected = true;
                Console.WriteLine($"Connected to server: {address} with id: {connectionId}");
            };

            // 3) Run host:
            host.Run();

            // 4) Connect to server:
            host.HostClient.Connect();

            // 5) Wait for connection (SpinWait only for simplifying asynchronous example):
            SpinWait.SpinUntil(() => isConnected, 120);

            // 4) Start sending messages:
            int i = 0;
            while (i < 100)
            {
                i++;
                host.HostClient.Send(new Message() { Text = $"Message {i} from client" }, ReliableChannel.Id);
                Thread.Sleep(1000);
            }

            Console.WriteLine("Press any key..");
            Console.ReadLine();
        }

        private static IHost BuildHost()
        {
            var hostSettings = new HostSettings(
                serializer: new Serializer(),
                loggerFactory: new SerilogLoggerFactory());

            return UdpHost
                .CreateHostBuilder()
                .ConfigureHost(hostSettings, settings =>
                {
                    settings.HostPorts = new[] { 5000 };
                })
                .ConfigureHostClient(settings =>
                {
                    settings.ServerPorts = new int[] { 7000 };
                })
                .BootstrapWorker(new HostWorkerGenerated())
                .Build();
        }
    }
}
```

## Up and running:
1) Run Server.
2) Run Client.
3) Check server logs, you should see the output:
```
[23:15:13 DBG] Message received: Message 1 from client
[23:15:13 DBG] Message received: Message 2 from client
[23:15:14 DBG] Message received: Message 3 from client
[23:15:15 DBG] Message received: Message 4 from client
[23:15:16 DBG] Message received: Message 5 from client
...
```

More samples available [Here](https://github.com/rdcm/UdpToolkit/tree/master/samples).

## Notes:

- This guide used popular .NET libraries [MessagePack-CSharp](https://github.com/neuecc/MessagePack-CSharp) and [Serilog](https://github.com/serilog/serilog) but UdpToolkit is dependency-free and you could use any other library for this goal after providing a custom implementation of `ISerializer` and `IUdpToolkitLoggerFactory`.
- `HostWorkerGenerated` - Generated by installed source generator: `UdpToolkit.CodeGenerator`. 
- An alternative way for explicit code generation is available through `udptoolkit-cli`, just install it `dotnet tool install --global UdpToolkit.CodeGeneratorCli`, this maybe useful for Unity3D projects. 
```
udptoolkit-cli \
-p path_to_project(csproj) \
-o path_to_output_file(HostWorkerGenerated.cs)
```