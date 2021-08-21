#!/bin/bash

dotnet restore ./UdpToolkit.sln --no-cache && dotnet publish ./UdpToolkit.sln -c Release -o ./server_build