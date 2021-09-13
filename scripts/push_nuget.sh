#!/bin/bash

dotnet nuget push ./packages/UdpToolkit.$PACKAGE_VERSION.nupkg                                       --api-key $NUGET_KEY --source https://api.nuget.org/v3/index.json
dotnet nuget push ./packages/UdpToolkit.Annotations.$PACKAGE_VERSION.nupkg                           --api-key $NUGET_KEY --source https://api.nuget.org/v3/index.json
dotnet nuget push ./packages/UdpToolkit.Framework.$PACKAGE_VERSION.nupkg                             --api-key $NUGET_KEY --source https://api.nuget.org/v3/index.json
dotnet nuget push ./packages/UdpToolkit.Framework.CodeGenerator.Contracts.$PACKAGE_VERSION.nupkg     --api-key $NUGET_KEY --source https://api.nuget.org/v3/index.json
dotnet nuget push ./packages/UdpToolkit.Framework.Contracts.$PACKAGE_VERSION.nupkg                   --api-key $NUGET_KEY --source https://api.nuget.org/v3/index.json
dotnet nuget push ./packages/UdpToolkit.Network.$PACKAGE_VERSION.nupkg                               --api-key $NUGET_KEY --source https://api.nuget.org/v3/index.json
dotnet nuget push ./packages/UdpToolkit.Network.Contracts.$PACKAGE_VERSION.nupkg                     --api-key $NUGET_KEY --source https://api.nuget.org/v3/index.json
dotnet nuget push ./packages/UdpToolkit.Serialization.$PACKAGE_VERSION.nupkg                         --api-key $NUGET_KEY --source https://api.nuget.org/v3/index.json
dotnet nuget push ./packages/UdpToolkit.Logging.$PACKAGE_VERSION.nupkg                               --api-key $NUGET_KEY --source https://api.nuget.org/v3/index.json

dotnet nuget push ./packages/UdpToolkit.CodeGenerator.$PACKAGE_VERSION.nupkg      --api-key $NUGET_KEY --source https://api.nuget.org/v3/index.json
dotnet nuget push ./packages/UdpToolkit.CodeGeneratorCli.$PACKAGE_VERSION.nupkg   --api-key $NUGET_KEY --source https://api.nuget.org/v3/index.json