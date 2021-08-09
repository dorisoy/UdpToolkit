#!/bin/bash

ls -la
dotnet restore ./UdpToolkit.sln --no-cache && dotnet publish ./UdpToolkit.sln -c Release -o ./server_build
