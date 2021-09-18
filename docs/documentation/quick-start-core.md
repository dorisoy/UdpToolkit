# Quick start guide (.NET Core)

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
Server/
  Server.csproj
Client/
  Client.cspproj
Contracts/
  Contracts.cspproj

SampleProject.sln
```

## Contracts
1) Update `Contracts.cspproj` file by the following code:  

```html
<Project Sdk="Microsoft.NET.Sdk">

    <PropertyGroup>
        <TargetFramework>net5.0</TargetFramework>
    </PropertyGroup>

    <ItemGroup>
        <PackageReference 
                Include="UdpToolkit.CodeGenerator" 
                Version="<latest_version>" 
                PrivateAssets="all" />
    </ItemGroup>

</Project>
```

2) Add class `Message` to library:

```c#
namespace Contracts
{
    using UdpToolkit.Annotations;


    [UdpEvent]
    public class Message
    {
        public string Text { get; set; }
    }
}
```

3) Add custom implementation of `ISerializer` to the library:
```c#
namespace Contracts
{
    using System;
    using System.Text.Json;
    using UdpToolkit.Serialization;

    public sealed class NetJsonSerializer : ISerializer
    {
        public byte[] Serialize<T>(T item)
        {
            return JsonSerializer.SerializeToUtf8Bytes(item);
        }

        public T Deserialize<T>(ArraySegment<byte> bytes)
        {
            return JsonSerializer.Deserialize<T>(bytes);
        }
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
        <PackageReference Include="UdpToolkit" Version="<latest_version>" />
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
    using UdpToolkit;
    using UdpToolkit.Framework;
    using UdpToolkit.Framework.Contracts;
    using UdpToolkit.Framework.Contracts.Settings;

    public static class Program
    {
        public static void Main(string[] args)
        {
            // 1) Create UDP host:
            var host = BuildHost();

            // 4) Subscribe on event:
            host.On<Message>(
                onEvent: (connectionId, ip, message) =>
                {
                    Console.WriteLine($"Message received: {message.Text}");
                    host.ServiceProvider.GroupManager
                        .JoinOrCreate(Guid.Empty, connectionId, ip);
                });

            // 5) Run host:
            host.Run();

            Console.WriteLine("Press any key..");
            Console.ReadLine();
        }

        private static IHost BuildHost()
        {
            var hostSettings = new HostSettings(
                serializer: new NetJsonSerializer());

            return UdpHost
                .CreateHostBuilder()
                .ConfigureHost(hostSettings, settings =>
                {
                    // 2) Assign port:
                    settings.HostPorts = new[] { 7000 };
                })
                .ConfigureNetwork((settings) =>
                {
                    // 3) Allow incoming connections:
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
        <PackageReference Include="UdpToolkit" Version="<latest_version>" />
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
    using UdpToolkit;
    using UdpToolkit.Framework;
    using UdpToolkit.Framework.Contracts;
    using UdpToolkit.Framework.Contracts.Settings;
    using UdpToolkit.Network.Channels;

    public static class Program
    {
        private static bool _isConnected = false;

        public static void Main(string[] args)
        {
            // 1) Create UDP host:
            var host = BuildHost();

            host.HostClient.OnConnected += (address, connectionId) =>
            {
                _isConnected = true;
                Console.WriteLine(
                    $"Connected to server: {address} " +
                    $"with id: {connectionId}");
            };

            // 4) Run host:
            host.Run();

            // 5) Connect to server:
            host.HostClient.Connect();

            // 6) Wait connection:
            SpinWait.SpinUntil(() => _isConnected, 120);

            // 5) Start sending messages:
            int i = 0;
            while (i < 100)
            {
                i++;
                host.HostClient.Send(
                    new Message
                    {
                        Text = $"Message {i} from client",
                    },
                    ReliableChannel.Id);
                Thread.Sleep(1000);
            }

            Console.WriteLine("Press any key..");
            Console.ReadLine();
        }

        private static IHost BuildHost()
        {
            var hostSettings = new HostSettings(
                serializer: new NetJsonSerializer());

            return UdpHost
                .CreateHostBuilder()
                .ConfigureHost(hostSettings, settings =>
                {
                    // 2) Assign port:
                    settings.HostPorts = new[] { 5000 };
                })
                .ConfigureHostClient(settings =>
                {
                    // 3) Specify remote host port:
                    settings.ServerPorts = new[] { 7000 };
                })
                .BootstrapWorker(new HostWorkerGenerated())
                .Build();
        }
    }
}
```

## Up and running
1) Run Server.
2) Run Client.
3) Observe output logs.
4) Enjoy :)

More samples available [Here](https://github.com/UdpToolkit/UdpToolkit/tree/master/samples).
