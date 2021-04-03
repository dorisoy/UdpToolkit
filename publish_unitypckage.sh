mkdir './unity_package'
mkdir './src/unity/UdpToolkit.Unity/Assets/Plugins/'
mkdir './src/unity/UdpToolkit.Unity/Assets/Plugins/UdpToolkit'

cp ./server_build/UdpToolkit.dll                       ./src/unity/UdpToolkit.Unity/Assets/Plugins/UdpToolkit/
cp ./server_build/UdpToolkit.Core.dll                  ./src/unity/UdpToolkit.Unity/Assets/Plugins/UdpToolkit/  
cp ./server_build/UdpToolkit.Network.dll               ./src/unity/UdpToolkit.Unity/Assets/Plugins/UdpToolkit/
cp ./server_build/UdpToolkit.Logging.dll               ./src/unity/UdpToolkit.Unity/Assets/Plugins/UdpToolkit/ 
cp ./server_build/UdpToolkit.Logging.Serilog.dll       ./src/unity/UdpToolkit.Unity/Assets/Plugins/UdpToolkit/
cp ./server_build/UdpToolkit.Serialization.dll         ./src/unity/UdpToolkit.Unity/Assets/Plugins/UdpToolkit/
cp ./server_build/UdpToolkit.Serialization.MsgPack.dll ./src/unity/UdpToolkit.Unity/Assets/Plugins/UdpToolkit/
cp ./server_build/Serilog.dll                          ./src/unity/UdpToolkit.Unity/Assets/Plugins/UdpToolkit/

/opt/Unity/Editor/Unity -quit -batchmode -nographics -projectPath ./src/unity/UdpToolkit.Unity/ -exportPackage "Assets/Plugins/UdpToolkit" "../../../unity_package/UdpToolkit.$PACKAGE_VERSION.unitypackage"