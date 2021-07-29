#!/bin/bash

dotnet pack ./src/shared/UdpToolkit/UdpToolkit.csproj -p:PackageVersion=$PACKAGE_VERSION --configuration Release
dotnet pack ./src/shared/UdpToolkit.Core/UdpToolkit.Core.csproj -p:PackageVersion=$PACKAGE_VERSION --configuration Release
dotnet pack ./src/shared/UdpToolkit.Network/UdpToolkit.Network.csproj -p:PackageVersion=$PACKAGE_VERSION --configuration Release
dotnet pack ./src/shared/UdpToolkit.Serialization/UdpToolkit.Serialization.csproj -p:PackageVersion=$PACKAGE_VERSION --configuration Release
dotnet pack ./src/shared/UdpToolkit.Serialization.MsgPack/UdpToolkit.Serialization.MsgPack.csproj -p:PackageVersion=$PACKAGE_VERSION --configuration Release
dotnet pack ./src/shared/UdpToolkit.Logging/UdpToolkit.Logging.csproj -p:PackageVersion=$PACKAGE_VERSION --configuration Release
dotnet pack ./src/shared/UdpToolkit.Logging.Serilog/UdpToolkit.Logging.Serilog.csproj -p:PackageVersion=$PACKAGE_VERSION --configuration Release