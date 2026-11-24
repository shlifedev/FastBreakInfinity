using System;
using System.IO;
using System.Linq;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Toolchains.CsProj;
using BenchmarkDotNet.Toolchains.DotNetCli;
using LD.Numeric.Benchmark;

var dotnetCliPath = GetHostPath("LD_NUMERIC_BENCHMARK_DOTNET") ?? GetHostPath("DOTNET_HOST_PATH");
var targetFramework = Environment.GetEnvironmentVariable("LD_NUMERIC_BENCHMARK_TFM");
var name = Environment.GetEnvironmentVariable("LD_NUMERIC_BENCHMARK_NAME");
IConfig config = DefaultConfig.Instance;
if (dotnetCliPath != null || targetFramework != null || name != null)
{
    var toolchain = CsProjCoreToolchain.From(
        new NetCoreAppSettings(
            targetFrameworkMoniker: targetFramework ?? "net10.0",
            runtimeFrameworkVersion: null,
            name: name ?? ".NET 10",
            customDotNetCliPath: dotnetCliPath
        )
    );
    config = config.AddJob(Job.Default.WithToolchain(toolchain));
}

var summaries = BenchmarkSwitcher
    .FromAssembly(typeof(BigDoubleBenchmarks).Assembly)
    .Run(args, config);
return summaries.Any(s => s.HasCriticalValidationErrors || s.Reports.Any(report => !report.Success))
    ? 1
    : 0;

static string? GetHostPath(string variable)
{
    var path = Environment.GetEnvironmentVariable(variable);
    return File.Exists(path) ? path : null;
}
