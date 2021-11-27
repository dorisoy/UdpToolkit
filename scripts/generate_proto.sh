#!/bin/sh
./protoc --version
./protoc -I=./benchmarks/UdpToolkit.Benchmarks/Sandbox/Serialization/ --csharp_out=./benchmarks/UdpToolkit.Benchmarks/Sandbox/Serialization/ ./benchmarks/UdpToolkit.Benchmarks/Sandbox/Serialization/person.proto