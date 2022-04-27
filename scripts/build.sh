#!/bin/bash

dotnet restore ./UdpToolkit.sln --no-cache && dotnet publish ./UdpToolkit.sln -c Release -p:ExtraDefineConstants=UNITY_BUILD -o ./build