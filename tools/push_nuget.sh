#!/bin/bash

dotnet nuget push ./src/shared/UdpToolkit/bin/Release/UdpToolkit.$PACKAGE_VERSION.nupkg                                             --api-key $NUGET_KEY --source https://api.nuget.org/v3/index.json
dotnet nuget push ./src/shared/UdpToolkit.Framework/bin/Release/UdpToolkit.Framework.$PACKAGE_VERSION.nupkg                         --api-key $NUGET_KEY --source https://api.nuget.org/v3/index.json
dotnet nuget push ./src/shared/UdpToolkit.Framework.Contracts/bin/Release/UdpToolkit.Framework.Contracts.$PACKAGE_VERSION.nupkg     --api-key $NUGET_KEY --source https://api.nuget.org/v3/index.json
dotnet nuget push ./src/shared/UdpToolkit.Network/bin/Release/UdpToolkit.Network.$PACKAGE_VERSION.nupkg                             --api-key $NUGET_KEY --source https://api.nuget.org/v3/index.json
dotnet nuget push ./src/shared/UdpToolkit.Network.Contracts/bin/Release/UdpToolkit.Network.Contracts.$PACKAGE_VERSION.nupkg         --api-key $NUGET_KEY --source https://api.nuget.org/v3/index.json
dotnet nuget push ./src/shared/UdpToolkit.Serialization/bin/Release/UdpToolkit.Serialization.$PACKAGE_VERSION.nupkg                 --api-key $NUGET_KEY --source https://api.nuget.org/v3/index.json
dotnet nuget push ./src/shared/UdpToolkit.Serialization.MsgPack/bin/Release/UdpToolkit.Serialization.MsgPack.$PACKAGE_VERSION.nupkg --api-key $NUGET_KEY --source https://api.nuget.org/v3/index.json
dotnet nuget push ./src/shared/UdpToolkit.Logging/bin/Release/UdpToolkit.Logging.$PACKAGE_VERSION.nupkg                             --api-key $NUGET_KEY --source https://api.nuget.org/v3/index.json
dotnet nuget push ./src/shared/UdpToolkit.Logging.Serilog/bin/Release/UdpToolkit.Logging.Serilog.$PACKAGE_VERSION.nupkg             --api-key $NUGET_KEY --source https://api.nuget.org/v3/index.json