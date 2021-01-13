#!/bin/bash

GREEN='\033[0;32m'
NC='\033[0m' # No Color

printf "${GREEN}Remove old msgpack generated formatter${NC}\n"
rm -f "./samples/Cubes/Cubes.Client/Assets/Cubes/Shared/MessagePackGenerated.cs"

printf "${GREEN}Clean old UdpToolkit${NC}\n"
rm -f "./samples/Cubes/Cubes.Client/Assets/Plugins/UdpToolkit.Core.deps.json"
rm -f "./samples/Cubes/Cubes.Client/Assets/Plugins/UdpToolkit.Core.deps.json.meta"
rm -f "./samples/Cubes/Cubes.Client/Assets/Plugins/UdpToolkit.Core.dll"
rm -f "./samples/Cubes/Cubes.Client/Assets/Plugins/UdpToolkit.Core.dll.meta"
rm -f "./samples/Cubes/Cubes.Client/Assets/Plugins/UdpToolkit.Core.pdb"
rm -f "./samples/Cubes/Cubes.Client/Assets/Plugins/UdpToolkit.Core.pdb.meta"

rm -f "./samples/Cubes/Cubes.Client/Assets/Plugins/UdpToolkit.Framework.deps.json"
rm -f "./samples/Cubes/Cubes.Client/Assets/Plugins/UdpToolkit.Framework.deps.json.meta"
rm -f "./samples/Cubes/Cubes.Client/Assets/Plugins/UdpToolkit.Framework.dll"
rm -f "./samples/Cubes/Cubes.Client/Assets/Plugins/UdpToolkit.Framework.dll.meta"
rm -f "./samples/Cubes/Cubes.Client/Assets/Plugins/UdpToolkit.Framework.pdb"
rm -f "./samples/Cubes/Cubes.Client/Assets/Plugins/UdpToolkit.Framework.pdb.meta"

rm -f "./samples/Cubes/Cubes.Client/Assets/Plugins/UdpToolkit.Network.deps.json"
rm -f "./samples/Cubes/Cubes.Client/Assets/Plugins/UdpToolkit.Network.deps.json.meta"
rm -f "./samples/Cubes/Cubes.Client/Assets/Plugins/UdpToolkit.Network.dll"
rm -f "./samples/Cubes/Cubes.Client/Assets/Plugins/UdpToolkit.Network.dll.meta"
rm -f "./samples/Cubes/Cubes.Client/Assets/Plugins/UdpToolkit.Network.pdb"
rm -f "./samples/Cubes/Cubes.Client/Assets/Plugins/UdpToolkit.Network.pdb.meta"

rm -f "./samples/Cubes/Cubes.Client/Assets/Plugins/UdpToolkit.Serialization.deps.json"
rm -f "./samples/Cubes/Cubes.Client/Assets/Plugins/UdpToolkit.Serialization.deps.json.meta"
rm -f "./samples/Cubes/Cubes.Client/Assets/Plugins/UdpToolkit.Serialization.dll"
rm -f "./samples/Cubes/Cubes.Client/Assets/Plugins/UdpToolkit.Serialization.dll.meta"
rm -f "./samples/Cubes/Cubes.Client/Assets/Plugins/UdpToolkit.Serialization.pdb"
rm -f "./samples/Cubes/Cubes.Client/Assets/Plugins/UdpToolkit.Serialization.pdb.meta"

rm -f "./samples/Cubes/Cubes.Client/Assets/Plugins/UdpToolkit.Serialization.MsgPack.deps.json"
rm -f "./samples/Cubes/Cubes.Client/Assets/Plugins/UdpToolkit.Serialization.MsgPack.deps.json.meta"
rm -f "./samples/Cubes/Cubes.Client/Assets/Plugins/UdpToolkit.Serialization.MsgPack.dll"
rm -f "./samples/Cubes/Cubes.Client/Assets/Plugins/UdpToolkit.Serialization.MsgPack.dll.meta"
rm -f "./samples/Cubes/Cubes.Client/Assets/Plugins/UdpToolkit.Serialization.MsgPack.pdb"
rm -f "./samples/Cubes/Cubes.Client/Assets/Plugins/UdpToolkit.Serialization.MsgPack.pdb.meta"

printf "${GREEN}Generate new formatters${NC}\n"
mpc -i "./samples/Cubes/Cubes.Shared/Cubes.Shared.csproj" -o "./samples/Cubes/Cubes.Client/Assets/Cubes/Shared/MessagePackGenerated.cs"

printf "${GREEN}Restore and Publish UdpToolkit${NC}\n"
dotnet restore --no-cache && dotnet publish -c Release -o ./server_build

printf "${GREEN}Copy UdpToolkit to Plugins${NC}\n"
cp ./server_build/UdpToolkit.Core.dll                  ./samples/Cubes/Cubes.Client/Assets/Plugins/ 
cp ./server_build/UdpToolkit.Framework.dll             ./samples/Cubes/Cubes.Client/Assets/Plugins/ 
cp ./server_build/UdpToolkit.Network.dll               ./samples/Cubes/Cubes.Client/Assets/Plugins/ 
cp ./server_build/UdpToolkit.Serialization.dll         ./samples/Cubes/Cubes.Client/Assets/Plugins/ 
cp ./server_build/UdpToolkit.Serialization.MsgPack.dll ./samples/Cubes/Cubes.Client/Assets/Plugins/

printf "${GREEN}Build and publish Cubes client${NC}\n"
/Applications/Unity/Hub/Editor/2019.3.12f1/Unity.app/Contents/MacOS/Unity -quit -batchmode -projectPath ./samples/Cubes/Cubes.Client/ -executeMethod BuildUtils.Build
 