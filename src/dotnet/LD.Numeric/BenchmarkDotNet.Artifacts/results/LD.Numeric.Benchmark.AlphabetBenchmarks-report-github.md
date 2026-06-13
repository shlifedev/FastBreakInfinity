```

BenchmarkDotNet v0.14.0, macOS 26.5.1 (25F80) [Darwin 25.5.0]
Apple M5 Pro, 1 CPU, 18 logical and 18 physical cores
.NET SDK 8.0.422 [/usr/local/share/dotnet/sdk]
  [Host]     : .NET 10.0.9 (10.0.926.27113), Arm64 RyuJIT AdvSIMD
  Job-XIYFBG : .NET 10.0.9 (10.0.926.27113), Arm64 RyuJIT AdvSIMD

Toolchain=.NET 10  IterationCount=1  WarmupCount=1  

```
| Method                               | Mean     | Error | Allocated |
|------------------------------------- |---------:|------:|----------:|
| AlphabetConverter_GetExponent_Double | 6.382 ns |    NA |         - |
