# Quick start guide (Unity 3D)

## Required tools

- Installed [net5.0](https://dotnet.microsoft.com/download/dotnet/5.0).
- UdpToolkit-Cli: `dotnet tool install --global UdpToolkit.Cli`.
- Latest version of [MessagePack-CSharp](https://github.com/neuecc/MessagePack-CSharp) and [mpc](https://github.com/neuecc/MessagePack-CSharp#aot-code-generation-support-for-unityxamarin).

## Project structure
1) Create class library:
   `dotnet new classlib --name Contracts`
2) Create Server:
   `dotnet new console --name Server`
3) Create Client (Empty Unity3d application with empty scene).
4) Create solution file and add all projects to them:
   `dotnet new sln --name SampleProject`

The final project structure should look like this:
```
Server/
  Server.csproj
Client/
  Assets/
  Client.sln
  ...(other Unity 3D folders)
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
      <PackageReference Include="MessagePack" Version="2.3.75" />
      <PackageReference Include="UdpToolkit.Framework.CodeGenerator.Contracts" Version="<latest_version>" />
   </ItemGroup>

   <ItemGroup>
      <Compile Include="../Client/Assets/Shared/**/*.cs" LinkBase="Shared" />
      <Compile Update="..\Client\Assets\Shared\Message.cs">
         <Link>Shared\Message.cs</Link>
      </Compile>
      <Compile Update="..\Client\Assets\Shared\MessagePackCSharpSerializer.cs">
         <Link>Shared\MessagePackSerializer.cs</Link>
      </Compile>
      <Compile Update="..\Client\Assets\Shared\HostWorkerGenerated.cs">
         <Link>Shared\HostWorkerGenerated.cs</Link>
      </Compile>
   </ItemGroup>

</Project>
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
    using Shared;
    using UdpToolkit;
    using UdpToolkit.Framework;
    using UdpToolkit.Framework.Contracts;

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

            // 5) Run host.:
            host.Run();

            Console.WriteLine("Press any key..");
            Console.ReadLine();
        }

        private static IHost BuildHost()
        {
            var hostSettings = new HostSettings(
                serializer: new MessagePackCSharpSerializer());

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

1) Install [UdpToolkit.unitypackage](https://github.com/UdpToolkit/UdpToolkit/releases) from release page.
2) Install [MessagePack.Unity.unitypackage](https://github.com/neuecc/MessagePack-CSharp/releases) from release page. At this point, conflicts between dll's possible, remove duplicates from `Plugins` folder.

3) Add following code to `Assets/Shared/Message.cs`:
```c#
// ReSharper disable once CheckNamespace

using MessagePack;
using UdpToolkit.Annotations;

namespace Shared
{
    [UdpEvent]
    [MessagePackObject]
    public class Message
    {
        [Key(0)]
        public string Text { get; set; }
    }
}
```
4) Generate `HostWorkerGenerated.cs` by command:  
`udptoolkit-cli -p ./Contracts/Contracts.csproj -o ./Client/Assets/Shared`.

5) Generate `MessagePack-CSharp` files by command:  
`mpc -i ./Contracts/Contracts.csproj -o ./Client/Assets/Scripts`.
7) Add custom implementation of `ISerializer` to `Assets/Shared/MessagePackCSharpSerializer.cs`: 
```c#
namespace Shared
{
    using MessagePack;
    using UdpToolkit.Serialization;

    public class MessagePackCSharpSerializer : ISerializer
    {
        public byte[] Serialize<T>(T item)
        {
            return MessagePackSerializer.Serialize(item);
        }

        public T Deserialize<T>(byte[] bytes)
        {
            return MessagePackSerializer.Deserialize<T>(bytes);
        }
    }
}
```

7) Add following code to `Assets/Scripts/ClientScript.cs`:
```c#
using MessagePack;
using MessagePack.Resolvers;
using Shared;
using UdpToolkit;
using UdpToolkit.Framework;
using UdpToolkit.Framework.Contracts;
using UdpToolkit.Logging;
using UdpToolkit.Network.Channels;
using UnityEngine;

public class ClientScript : MonoBehaviour
{
    private bool _isConnected;
    private IHost _host;
    private int _counter = 0;

    void Start()
    {
        // 1) Register serializers:
        StaticCompositeResolver.Instance.Register(
            GeneratedResolver.Instance,
            StandardResolver.Instance);

        MessagePackSerializer.DefaultOptions = MessagePackSerializerOptions.Standard
            .WithResolver(StaticCompositeResolver.Instance);
        
        // 2) Create UDP host:
        _host = BuildHost();

        _host.HostClient.OnConnected += (address, connectionId) =>
        {
            _isConnected = true;
            Debug.Log(
                $"Connected to server: {address} " +
                $"with id: {connectionId}");
        };

        // 6) Run host:
        _host.Run();

        // 7) Connect to server:
        _host.HostClient.Connect();
    }

    void Update()
    {
        _counter++;
        if (_isConnected)
        {
            if (_counter % 60 == 0)
            {
                _host.HostClient.Send(
                    new Shared.Message
                    {
                        Text = $"Message from Unity3D client",
                    },
                    ReliableChannel.Id);
            }
        }
    }

    private void OnDestroy()
    {
        _host.Dispose();
    }

    private static IHost BuildHost()
    {
        var hostSettings = new HostSettings(
            serializer: new MessagePackCSharpSerializer());

        return UdpHost
            .CreateHostBuilder()
            .ConfigureHost(hostSettings, settings =>
            {
                // 3) Assign port
                settings.HostPorts = new[] { 5000 };
                // 4) Replace default logger implementation 
                settings.LoggerFactory = new UnityLoggerFactory(LogLevel.Debug);
            })
            .ConfigureHostClient(settings =>
            {
                // 5) Specify remote host port
                settings.ServerPorts = new[] { 7000 };
            })
            .BootstrapWorker(new HostWorkerGenerated())
            .Build();
    }
}
```

8) Create empty `GameObject` on scene and attach script `ClientScript.cs`.

## Up and running
1) Run Server.
2) Run Client.
3) Observe output logs.
4) Enjoy :)

More samples available [Here](https://github.com/UdpToolkit/UdpToolkit/tree/master/samples).