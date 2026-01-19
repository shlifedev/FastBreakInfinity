using BenchmarkDotNet.Running;
using LD.Numeric.Benchmark;

BenchmarkSwitcher.FromAssembly(typeof(BigDoubleBenchmarks).Assembly).Run(args);
