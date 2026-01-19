```

BenchmarkDotNet v0.14.0, macOS 26.2 (25C56) [Darwin 25.2.0]
Apple M1 Pro, 1 CPU, 10 logical and 10 physical cores
.NET SDK 10.0.102
  [Host]     : .NET 10.0.2 (10.0.226.5608), Arm64 RyuJIT AdvSIMD
  DefaultJob : .NET 10.0.2 (10.0.226.5608), Arm64 RyuJIT AdvSIMD


```
| Method                           | Mean        | Error     | StdDev    | Gen0   | Allocated |
|--------------------------------- |------------:|----------:|----------:|-------:|----------:|
| Arithmetic_Add_SameExponent      |   7.1741 ns | 0.0673 ns | 0.0597 ns |      - |         - |
| Arithmetic_Add_DifferentExponent |   2.0793 ns | 0.0332 ns | 0.0295 ns |      - |         - |
| Arithmetic_Subtract              |   2.0662 ns | 0.0177 ns | 0.0165 ns |      - |         - |
| Arithmetic_Multiply              |   4.9368 ns | 0.0451 ns | 0.0377 ns |      - |         - |
| Arithmetic_Divide                |  16.6339 ns | 0.0541 ns | 0.0506 ns |      - |         - |
| Arithmetic_Reciprocate           |   5.0079 ns | 0.0725 ns | 0.0679 ns |      - |         - |
| Comparison_Equals                |   0.9441 ns | 0.0069 ns | 0.0057 ns |      - |         - |
| Comparison_LessThan              |   1.9839 ns | 0.0072 ns | 0.0067 ns |      - |         - |
| Comparison_GreaterThan           |   2.0157 ns | 0.0228 ns | 0.0178 ns |      - |         - |
| Comparison_CompareTo             |   1.6818 ns | 0.0079 ns | 0.0074 ns |      - |         - |
| Comparison_Max                   |   2.0863 ns | 0.0253 ns | 0.0236 ns |      - |         - |
| Comparison_Min                   |   2.0942 ns | 0.0196 ns | 0.0183 ns |      - |         - |
| Constructor_FromDouble           |   6.1951 ns | 0.0404 ns | 0.0358 ns |      - |         - |
| Constructor_FromString           |   9.0920 ns | 0.0549 ns | 0.0429 ns |      - |         - |
| Constructor_FromMantissaExponent |   0.5058 ns | 0.0041 ns | 0.0038 ns |      - |         - |
| Conversion_ToString_Small        | 110.1663 ns | 0.9912 ns | 0.8277 ns | 0.0050 |      32 B |
| Conversion_ToString_Large        |  68.9228 ns | 0.7256 ns | 0.6059 ns | 0.0114 |      72 B |
| Conversion_ToString_VeryLarge    |  68.6502 ns | 0.6224 ns | 0.5822 ns | 0.0114 |      72 B |
| Conversion_ToDouble              |   1.5124 ns | 0.0054 ns | 0.0042 ns |      - |         - |
| Conversion_AdjustedMantissa      |   8.8734 ns | 0.0269 ns | 0.0225 ns |      - |         - |
| Conversion_GetAlphabetUnit       |   2.9914 ns | 0.0277 ns | 0.0259 ns |      - |         - |
| Conversion_GetAlphabetUnit_Large |   3.0035 ns | 0.0139 ns | 0.0130 ns |      - |         - |
| Math_Pow_SmallExponent           |   8.6956 ns | 0.0235 ns | 0.0220 ns |      - |         - |
| Math_Pow_LargeExponent           |  17.8841 ns | 0.0485 ns | 0.0454 ns |      - |         - |
| Math_Pow_DoubleExponent          |  23.2980 ns | 0.0940 ns | 0.0833 ns |      - |         - |
| Math_Pow10_Integer               |   0.0000 ns | 0.0000 ns | 0.0000 ns |      - |         - |
| Math_Pow10_Double                |   0.1790 ns | 0.0017 ns | 0.0015 ns |      - |         - |
| Math_Log10                       |   2.7311 ns | 0.0033 ns | 0.0031 ns |      - |         - |
| Math_Log_Base2                   |   2.7577 ns | 0.0053 ns | 0.0044 ns |      - |         - |
| Math_Log2                        |   2.7500 ns | 0.0047 ns | 0.0044 ns |      - |         - |
| Math_Ln                          |   2.8443 ns | 0.0034 ns | 0.0032 ns |      - |         - |
| Math_Sqrt                        |   2.0749 ns | 0.0134 ns | 0.0118 ns |      - |         - |
| Math_Cbrt                        |  10.2161 ns | 0.0233 ns | 0.0218 ns |      - |         - |
| Math_Exp                         |  44.5171 ns | 0.1006 ns | 0.0892 ns |      - |         - |
| Math_Factorial                   | 117.9719 ns | 0.2917 ns | 0.2729 ns |      - |         - |
| Math_Abs                         |   0.0000 ns | 0.0000 ns | 0.0000 ns |      - |         - |
| Rounding_Round                   |   9.0474 ns | 0.0259 ns | 0.0243 ns |      - |         - |
| Rounding_Floor                   |   9.3469 ns | 0.0271 ns | 0.0254 ns |      - |         - |
| Rounding_Ceiling                 |   9.5096 ns | 0.0357 ns | 0.0334 ns |      - |         - |
| Rounding_Truncate                |   9.0451 ns | 0.0367 ns | 0.0343 ns |      - |         - |
