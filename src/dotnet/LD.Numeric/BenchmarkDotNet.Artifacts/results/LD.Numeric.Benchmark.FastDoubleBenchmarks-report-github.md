```

BenchmarkDotNet v0.14.0, macOS 26.2 (25C56) [Darwin 25.2.0]
Apple M1 Pro, 1 CPU, 10 logical and 10 physical cores
.NET SDK 10.0.102
  [Host]     : .NET 10.0.2 (10.0.226.5608), Arm64 RyuJIT AdvSIMD
  DefaultJob : .NET 10.0.2 (10.0.226.5608), Arm64 RyuJIT AdvSIMD


```
| Method                         | Mean       | Error     | StdDev    | Gen0   | Allocated |
|------------------------------- |-----------:|----------:|----------:|-------:|----------:|
| Parse_DoubleParse_Simple       |  26.488 ns | 0.1054 ns | 0.0986 ns |      - |         - |
| Parse_FastDouble_Simple        |   5.620 ns | 0.0188 ns | 0.0176 ns |      - |         - |
| Parse_DoubleParse_Decimal      |  40.188 ns | 0.1752 ns | 0.1639 ns |      - |         - |
| Parse_FastDouble_Decimal       |  21.633 ns | 0.0893 ns | 0.0836 ns |      - |         - |
| Parse_DoubleParse_Scientific   |  33.838 ns | 0.0935 ns | 0.0874 ns |      - |         - |
| Parse_FastDouble_Scientific    |  22.306 ns | 0.0983 ns | 0.0919 ns |      - |         - |
| Parse_DoubleParse_Negative     |  32.210 ns | 0.1337 ns | 0.1251 ns |      - |         - |
| Parse_FastDouble_Negative      |  15.863 ns | 0.0809 ns | 0.0756 ns |      - |         - |
| Parse_FastDouble_Precision3    |  16.366 ns | 0.0837 ns | 0.0783 ns |      - |         - |
| Parse_FastDouble_Precision6    |  21.613 ns | 0.0838 ns | 0.0784 ns |      - |         - |
| Parse_FastDouble_Precision10   |  21.654 ns | 0.0813 ns | 0.0761 ns |      - |         - |
| ToString_DoubleToString_Small  |  87.772 ns | 0.3940 ns | 0.3686 ns | 0.0063 |      40 B |
| ToString_FastDouble_Small      |  29.231 ns | 0.1537 ns | 0.1437 ns | 0.0063 |      40 B |
| ToString_DoubleToString_Medium | 121.225 ns | 0.6601 ns | 0.6175 ns | 0.0062 |      40 B |
| ToString_FastDouble_Medium     |  33.975 ns | 0.1960 ns | 0.1833 ns | 0.0063 |      40 B |
| ToString_DoubleToString_Large  | 129.626 ns | 0.5709 ns | 0.5340 ns | 0.0076 |      48 B |
| ToString_FastDouble_Large      |  35.739 ns | 0.2830 ns | 0.2363 ns | 0.0076 |      48 B |
