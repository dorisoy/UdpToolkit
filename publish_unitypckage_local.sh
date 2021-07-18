dotnet restore --no-cache && dotnet publish -c Release -o ./server_build

mkdir -p './unity_package'
mkdir -p './src/unity/UdpToolkit.Unity/Assets/Plugins/'
mkdir -p './src/unity/UdpToolkit.Unity/Assets/Plugins/UdpToolkit'

cp ./server_build/UdpToolkit.dll                       ./src/unity/UdpToolkit.Unity/Assets/Plugins/UdpToolkit/
cp ./server_build/UdpToolkit.Core.dll                  ./src/unity/UdpToolkit.Unity/Assets/Plugins/UdpToolkit/  
cp ./server_build/UdpToolkit.Network.dll               ./src/unity/UdpToolkit.Unity/Assets/Plugins/UdpToolkit/
cp ./server_build/UdpToolkit.Logging.dll               ./src/unity/UdpToolkit.Unity/Assets/Plugins/UdpToolkit/ 
cp ./server_build/UdpToolkit.Logging.Serilog.dll       ./src/unity/UdpToolkit.Unity/Assets/Plugins/UdpToolkit/
cp ./server_build/UdpToolkit.Serialization.dll         ./src/unity/UdpToolkit.Unity/Assets/Plugins/UdpToolkit/
cp ./server_build/UdpToolkit.Serialization.MsgPack.dll ./src/unity/UdpToolkit.Unity/Assets/Plugins/UdpToolkit/
cp ./server_build/Serilog.dll                          ./src/unity/UdpToolkit.Unity/Assets/Plugins/UdpToolkit/

/Applications/Unity/Hub/Editor/2019.4.20f1/Unity.app/Contents/MacOS/Unity -quit -batchmode -nographics -projectPath ./src/unity/UdpToolkit.Unity/ -exportPackage "Assets/Plugins/UdpToolkit" "../../../unity_package/UdpToolkit.$PACKAGE_VERSION.unitypackage"