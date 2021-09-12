#!/bin/bash

rm -f ./BenchmarkDotNet.Artifacts && rm -f ./benchmarks_build
dotnet build ./benchmarks/UdpToolkit.Benchmarks/UdpToolkit.Benchmarks.csproj -c release -o ./benchmarks_build  
sudo dotnet ./benchmarks_build/UdpToolkit.Benchmarks.dll

open ./BenchmarkDotNet.Artifacts/results/UdpToolkit.Benchmarks.SubscriptionBenchmark-report.html