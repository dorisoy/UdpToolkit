#!/bin/bash

# folders for managed part
mkdir './unity_package'

# folders for native part
mkdir -p './src/unity/UdpToolkit.Unity/Assets/Plugins/UdpToolkit/runtimes/linux-x64/native'
mkdir -p './src/unity/UdpToolkit.Unity/Assets/Plugins/UdpToolkit/runtimes/osx-x64/native'
mkdir -p './src/unity/UdpToolkit.Unity/Assets/Plugins/UdpToolkit/runtimes/win-x64/native'

# copy managed part
cp ./server_build/UdpToolkit.dll                       ./src/unity/UdpToolkit.Unity/Assets/Plugins/UdpToolkit/
cp ./server_build/UdpToolkit.Framework.dll             ./src/unity/UdpToolkit.Unity/Assets/Plugins/UdpToolkit/
cp ./server_build/UdpToolkit.Framework.Contracts.dll    ./src/unity/UdpToolkit.Unity/Assets/Plugins/UdpToolkit/
cp ./server_build/UdpToolkit.Network.dll               ./src/unity/UdpToolkit.Unity/Assets/Plugins/UdpToolkit/  
cp ./server_build/UdpToolkit.Network.Contracts.dll     ./src/unity/UdpToolkit.Unity/Assets/Plugins/UdpToolkit/
cp ./server_build/UdpToolkit.Logging.dll               ./src/unity/UdpToolkit.Unity/Assets/Plugins/UdpToolkit/ 
cp ./server_build/UdpToolkit.Serialization.dll         ./src/unity/UdpToolkit.Unity/Assets/Plugins/UdpToolkit/
cp ./server_build/UdpToolkit.Serialization.MsgPack.dll ./src/unity/UdpToolkit.Unity/Assets/Plugins/UdpToolkit/

# copy native part
cp ./server_build/runtimes/linux-x64/native/udp_toolkit_native.so    ./src/unity/UdpToolkit.Unity/Assets/Plugins/UdpToolkit/runtimes/linux-x64/native/
cp ./server_build/runtimes/osx-x64/native/udp_toolkit_native.dylib   ./src/unity/UdpToolkit.Unity/Assets/Plugins/UdpToolkit/runtimes/osx-x64/native/
cp ./server_build/runtimes/win-x64/native/udp_toolkit_native.dll     ./src/unity/UdpToolkit.Unity/Assets/Plugins/UdpToolkit/runtimes/win-x64/native/

# build unity package
/opt/Unity/Editor/Unity -quit -batchmode -nographics -projectPath ./src/unity/UdpToolkit.Unity/ -exportPackage "Assets/Plugins/UdpToolkit" "../../../unity_package/UdpToolkit.$PACKAGE_VERSION.unitypackage"