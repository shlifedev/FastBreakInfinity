using System;
using System.IO;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Toolchains.CsProj;
using BenchmarkDotNet.Toolchains.DotNetCli;
using BenchmarkDotNet.Toolchains.MonoAotLLVM;
using LD.Numeric.Benchmark;

var dotnetCliPath =
    Environment.GetEnvironmentVariable("LD_NUMERIC_BENCHMARK_DOTNET")
    ?? Environment.GetEnvironmentVariable("DOTNET_HOST_PATH");
if (string.IsNullOrWhiteSpace(dotnetCliPath) || !File.Exists(dotnetCliPath))
{
    dotnetCliPath = "/Users/shlifedev/.dotnet/dotnet";
}

var toolchain = CsProjCoreToolchain.From(
    new NetCoreAppSettings(
        targetFrameworkMoniker: "net10.0",
        runtimeFrameworkVersion: null,
        name: ".NET 10",
        customDotNetCliPath: dotnetCliPath,
        packagesPath: null,
        customRuntimePack: null,
        aotCompilerPath: null,
        aotCompilerMode: default
    )
);

var config = DefaultConfig.Instance.AddJob(Job.Default.WithToolchain(toolchain));

BenchmarkSwitcher.FromAssembly(typeof(BigDoubleBenchmarks).Assembly).Run(args, config);
