#!/bin/bash

dotnet restore --no-cache && dotnet publish -c Release -o ./server_build

dotnet pack ./src/shared/UdpToolkit/UdpToolkit.csproj -p:PackageVersion=$PACKAGE_VERSION --configuration Release
dotnet pack ./src/shared/UdpToolkit.Core/UdpToolkit.Core.csproj -p:PackageVersion=$PACKAGE_VERSION --configuration Release
dotnet pack ./src/shared/UdpToolkit.Network/UdpToolkit.Network.csproj -p:PackageVersion=$PACKAGE_VERSION --configuration Release
dotnet pack ./src/shared/UdpToolkit.Serialization/UdpToolkit.Serialization.csproj -p:PackageVersion=$PACKAGE_VERSION --configuration Release
dotnet pack ./src/shared/UdpToolkit.Serialization.MsgPack/UdpToolkit.Serialization.MsgPack.csproj -p:PackageVersion=$PACKAGE_VERSION --configuration Release
dotnet pack ./src/shared/UdpToolkit.Logging/UdpToolkit.Logging.csproj -p:PackageVersion=$PACKAGE_VERSION --configuration Release
dotnet pack ./src/shared/UdpToolkit.Logging.Serilog/UdpToolkit.Logging.Serilog.csproj -p:PackageVersion=$PACKAGE_VERSION --configuration Release

dotnet nuget push ./src/shared/UdpToolkit/bin/Release/UdpToolkit.$PACKAGE_VERSION.nupkg --api-key $NUGET_KEY --source https://api.nuget.org/v3/index.json
dotnet nuget push ./src/shared/UdpToolkit.Core/bin/Release/UdpToolkit.Core.$PACKAGE_VERSION.nupkg --api-key $NUGET_KEY --source https://api.nuget.org/v3/index.json
dotnet nuget push ./src/shared/UdpToolkit.Network/bin/Release/UdpToolkit.Network.$PACKAGE_VERSION.nupkg --api-key $NUGET_KEY --source https://api.nuget.org/v3/index.json
dotnet nuget push ./src/shared/UdpToolkit.Serialization/bin/Release/UdpToolkit.Serialization.$PACKAGE_VERSION.nupkg --api-key $NUGET_KEY --source https://api.nuget.org/v3/index.json
dotnet nuget push ./src/shared/UdpToolkit.Serialization.MsgPack/bin/Release/UdpToolkit.Serialization.MsgPack.$PACKAGE_VERSION.nupkg --api-key $NUGET_KEY --source https://api.nuget.org/v3/index.json
dotnet nuget push ./src/shared/UdpToolkit.Logging/bin/Release/UdpToolkit.Logging.$PACKAGE_VERSION.nupkg --api-key $NUGET_KEY --source https://api.nuget.org/v3/index.json
dotnet nuget push ./src/shared/UdpToolkit.Logging.Serilog/bin/Release/UdpToolkit.Logging.Serilog.$PACKAGE_VERSION.nupkg --api-key $NUGET_KEY --source https://api.nuget.org/v3/index.json