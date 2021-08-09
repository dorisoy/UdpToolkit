#!/bin/bash

# folders for managed part
mkdir './unity_package'

./tools/copy_dlls_to_unity.sh

# build unity package
/opt/Unity/Editor/Unity -quit -batchmode -nographics -projectPath ./src/unity/UdpToolkit.Unity/ -exportPackage "Assets/Plugins/UdpToolkit" "../../../unity_package/UdpToolkit.$PACKAGE_VERSION.unitypackage"
