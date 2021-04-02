#!/bin/bash

sudo rm -f ./BenchmarkDotNet.Artifacts && rm -f ./benchmarks_build
sudo dotnet build ./benchmarks/UdpToolkit.Benchmarks/UdpToolkit.Benchmarks.csproj -c release -o ./benchmarks_build  
sudo dotnet ./benchmarks_build/UdpToolkit.Benchmarks.dll

open ./BenchmarkDotNet.Artifacts/results/UdpToolkit.Benchmarks.UdpClientBenchmark-report.html
open ./BenchmarkDotNet.Artifacts/results/UdpToolkit.Benchmarks.ProducerConsumerQueueBenchmark-report.html
open ./BenchmarkDotNet.Artifacts/results/UdpToolkit.Benchmarks.GenericSerializationBenchmark-report.html
open ./BenchmarkDotNet.Artifacts/results/UdpToolkit.Benchmarks.ProtocolSerializationBenchmark-report.html
open ./BenchmarkDotNet.Artifacts/results/UdpToolkit.Benchmarks.UdpReceiveBenchmark-report.html