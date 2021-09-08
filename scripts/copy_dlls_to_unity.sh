#!/bin/bash

# folders for native part
mkdir -p './src/unity/UdpToolkit.Unity/Assets/Plugins/UdpToolkit/runtimes/linux-x64/native'
mkdir -p './src/unity/UdpToolkit.Unity/Assets/Plugins/UdpToolkit/runtimes/osx-x64/native'
mkdir -p './src/unity/UdpToolkit.Unity/Assets/Plugins/UdpToolkit/runtimes/win-x64/native'

# copy managed part
cp ./build/UdpToolkit.dll                                     ./src/unity/UdpToolkit.Unity/Assets/Plugins/UdpToolkit/
cp ./build/UdpToolkit.Annotations.dll                         ./src/unity/UdpToolkit.Unity/Assets/Plugins/UdpToolkit/
cp ./build/UdpToolkit.Framework.dll                           ./src/unity/UdpToolkit.Unity/Assets/Plugins/UdpToolkit/
cp ./build/UdpToolkit.Framework.CodeGenerator.Contracts.dll   ./src/unity/UdpToolkit.Unity/Assets/Plugins/UdpToolkit/
cp ./build/UdpToolkit.Framework.Contracts.dll                 ./src/unity/UdpToolkit.Unity/Assets/Plugins/UdpToolkit/
cp ./build/UdpToolkit.Logging.dll                             ./src/unity/UdpToolkit.Unity/Assets/Plugins/UdpToolkit/
cp ./build/UdpToolkit.Network.dll                             ./src/unity/UdpToolkit.Unity/Assets/Plugins/UdpToolkit/  
cp ./build/UdpToolkit.Network.Contracts.dll                   ./src/unity/UdpToolkit.Unity/Assets/Plugins/UdpToolkit/
cp ./build/UdpToolkit.Serialization.dll                       ./src/unity/UdpToolkit.Unity/Assets/Plugins/UdpToolkit/

# copy system dll's
cp ./build/System.Buffers.dll                          ./src/unity/UdpToolkit.Unity/Assets/Plugins/UdpToolkit/
cp ./build/System.Memory.dll                           ./src/unity/UdpToolkit.Unity/Assets/Plugins/UdpToolkit/
cp ./build/System.Runtime.CompilerServices.Unsafe.dll  ./src/unity/UdpToolkit.Unity/Assets/Plugins/UdpToolkit/

# copy native part
cp ./build/runtimes/linux-x64/native/udp_toolkit_native.so    ./src/unity/UdpToolkit.Unity/Assets/Plugins/UdpToolkit/runtimes/linux-x64/native/
cp ./build/runtimes/osx-x64/native/udp_toolkit_native.dylib   ./src/unity/UdpToolkit.Unity/Assets/Plugins/UdpToolkit/runtimes/osx-x64/native/
cp ./build/runtimes/win-x64/native/udp_toolkit_native.dll     ./src/unity/UdpToolkit.Unity/Assets/Plugins/UdpToolkit/runtimes/win-x64/native/
