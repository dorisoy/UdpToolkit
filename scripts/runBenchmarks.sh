#!/bin/bash

rm -f ./BenchmarkDotNet.Artifacts && rm -f ./benchmarks_build
dotnet build ./benchmarks/UdpToolkit.Benchmarks/UdpToolkit.Benchmarks.csproj -c release -o ./benchmarks_build  
dotnet ./benchmarks_build/UdpToolkit.Benchmarks.dll

# Library
open ./BenchmarkDotNet.Artifacts/results/UdpToolkit.Benchmarks.SubscriptionBenchmark-report.html
open ./BenchmarkDotNet.Artifacts/results/UdpToolkit.Benchmarks.UnsafeSerializationBenchmark-report.html
open ./BenchmarkDotNet.Artifacts/results/UdpToolkit.Benchmarks.UdpClientBenchmark-report.html

# Pooling
open ./BenchmarkDotNet.Artifacts/results/UdpToolkit.Benchmarks.Sandbox.Pooling.MemoryPoolBenchmark-report.html
open ./BenchmarkDotNet.Artifacts/results/UdpToolkit.Benchmarks.Sandbox.Pooling.ArrayPoolBenchmark-report.html

# Serialization
open ./BenchmarkDotNet.Artifacts/results/UdpToolkit.Benchmarks.Sandbox.Serialization.NetProtobufSerializationBenchmark-report.html
open ./BenchmarkDotNet.Artifacts/results/UdpToolkit.Benchmarks.Sandbox.Serialization.MessagePackSerializationBenchmark-report.html
open ./BenchmarkDotNet.Artifacts/results/UdpToolkit.Benchmarks.Sandbox.Serialization.ProtobufSerializationBenchmark-report.html

# Queues
open ./BenchmarkDotNet.Artifacts/results/UdpToolkit.Benchmarks.Sandbox.Queues.ChannelBenchmark-report.html