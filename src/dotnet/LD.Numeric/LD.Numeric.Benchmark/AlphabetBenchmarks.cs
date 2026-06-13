using System.Numerics;
using BenchmarkDotNet.Attributes;
using LD.Numeric.IdleNumber;

namespace LD.Numeric.Benchmark;

[MemoryDiagnoser]
public class AlphabetBenchmarks
{
    private long _smallIndex;
    private long _largeIndex;
    private long _exponent;
    private double _largeDouble;
    private double _alphabetUnitDouble;
    private double _roundingCarryDouble;
    private double _digitsSmallDouble;
    private string _alphabetUnitString = null!;
    private string _largeUnit = null!;
    private BigInteger _largeBigInteger;

    [GlobalSetup]
    public void Setup()
    {
        AlphabetManager.Reset();

        _smallIndex = 12345L;
        _largeIndex = 10_000_000_000L;
        _exponent = 123456L;
        _largeDouble = 9.87654321e123;
        _alphabetUnitDouble = 9_876_543.21;
        _roundingCarryDouble = 999_999.0;
        _digitsSmallDouble = 123.456;
        _alphabetUnitString = "1.23AA";
        _largeUnit = AlphabetManager.GetAlphabetUnit(_largeIndex);
        _largeBigInteger = BigInteger.Pow(10, 60);

        AlphabetManager.GetAlphabetUnit(_smallIndex);
        AlphabetManager.GetAlphabetUnitFromExponent(_exponent);
        AlphabetManager.GetIndexFromUnit(_largeUnit);
    }

    #region AlphabetManager

    [BenchmarkCategory("AlphabetManager")]
    [Benchmark]
    public string AlphabetManager_GetAlphabetUnit_CachedSmall()
    {
        return AlphabetManager.GetAlphabetUnit(_smallIndex);
    }

    [BenchmarkCategory("AlphabetManager")]
    [Benchmark]
    public string AlphabetManager_GetAlphabetUnit_CachedLarge()
    {
        return AlphabetManager.GetAlphabetUnit(_largeIndex);
    }

    [BenchmarkCategory("AlphabetManager")]
    [Benchmark]
    public string AlphabetManager_GetAlphabetUnit_ResetAndBuildLarge()
    {
        AlphabetManager.Reset();
        return AlphabetManager.GetAlphabetUnit(_largeIndex);
    }

    [BenchmarkCategory("AlphabetManager")]
    [Benchmark]
    public long AlphabetManager_GetIndexFromUnit_CachedLarge()
    {
        return AlphabetManager.GetIndexFromUnit(_largeUnit);
    }

    [BenchmarkCategory("AlphabetManager")]
    [Benchmark]
    public string AlphabetManager_GetAlphabetUnitFromExponent()
    {
        return AlphabetManager.GetAlphabetUnitFromExponent(_exponent);
    }

    #endregion

    #region AlphabetConverter

    [BenchmarkCategory("AlphabetConverter")]
    [Benchmark]
    public long AlphabetConverter_GetExponent_Double()
    {
        return AlphabetConverter.GetExponent(_largeDouble);
    }

    [BenchmarkCategory("AlphabetConverter")]
    [Benchmark]
    public long AlphabetConverter_GetExponent_BigInteger()
    {
        return AlphabetConverter.GetExponent(_largeBigInteger);
    }

    [BenchmarkCategory("AlphabetConverter")]
    [Benchmark]
    public string AlphabetConverter_ConvertToAlphabetUnit()
    {
        return _alphabetUnitDouble.ConvertToAlphabetUnit();
    }

    [BenchmarkCategory("AlphabetConverter")]
    [Benchmark]
    public string AlphabetConverter_ConvertToAlphabetUnit_RoundingCarry()
    {
        return _roundingCarryDouble.ConvertToAlphabetUnit();
    }

    [BenchmarkCategory("AlphabetConverter")]
    [Benchmark]
    public double AlphabetConverter_ConvertFromAlphabetUnit()
    {
        return AlphabetConverter.ConvertFromAlphabetUnit(_alphabetUnitString);
    }

    #endregion

    #region NumberUtility

    [BenchmarkCategory("NumberUtility")]
    [Benchmark]
    public int NumberUtility_GetDigits_Small()
    {
        return NumberUtility.GetDigits(_digitsSmallDouble);
    }

    [BenchmarkCategory("NumberUtility")]
    [Benchmark]
    public int NumberUtility_GetDigits_Large()
    {
        return NumberUtility.GetDigits(_largeDouble);
    }

    #endregion
}
