using System;
using System.Globalization;
using BenchmarkDotNet.Attributes;
using LD.Numeric.IdleNumber;

namespace LD.Numeric.Benchmark;

[MemoryDiagnoser]
public class FastDoubleBenchmarks
{
    private string _simpleNumber = null!;
    private string _decimalNumber = null!;
    private string _scientificNumber = null!;
    private string _smallScientificNumber = null!;
    private string _longScientificNumber = null!;
    private string _negativeNumber = null!;
    private string _sliceSource = null!;

    private double _smallDouble;
    private double _mediumDouble;
    private double _largeDouble;
    private double _roundingCarryDouble;

    [GlobalSetup]
    public void Setup()
    {
        _simpleNumber = "12345";
        _decimalNumber = "123.456789";
        _scientificNumber = "1.5e10";
        _smallScientificNumber = "0.0000001234567e10";
        _longScientificNumber = "1.23456789e10";
        _negativeNumber = "-987.654";
        _sliceSource = "price:1.5e3";

        _smallDouble = 123.456;
        _mediumDouble = 12345.6789;
        _largeDouble = 9876543.21;
        _roundingCarryDouble = 999.995;
    }

    #region ParseDouble vs double.Parse

    [BenchmarkCategory("Parse")]
    [Benchmark]
    public double Parse_DoubleParse_Simple()
    {
        return double.Parse(_simpleNumber, CultureInfo.InvariantCulture);
    }

    [BenchmarkCategory("Parse")]
    [Benchmark]
    public double Parse_FastDouble_Simple()
    {
        return FastDouble.ParseDouble(_simpleNumber);
    }

    [BenchmarkCategory("Parse")]
    [Benchmark]
    public double Parse_DoubleParse_Decimal()
    {
        return double.Parse(_decimalNumber, CultureInfo.InvariantCulture);
    }

    [BenchmarkCategory("Parse")]
    [Benchmark]
    public double Parse_FastDouble_Decimal()
    {
        return FastDouble.ParseDouble(_decimalNumber);
    }

    [BenchmarkCategory("Parse")]
    [Benchmark]
    public double Parse_DoubleParse_Scientific()
    {
        return double.Parse(_scientificNumber, CultureInfo.InvariantCulture);
    }

    [BenchmarkCategory("Parse")]
    [Benchmark]
    public double Parse_FastDouble_Scientific()
    {
        return FastDouble.ParseDouble(_scientificNumber);
    }

    [BenchmarkCategory("Parse")]
    [Benchmark]
    public double Parse_DoubleParse_SmallScientific()
    {
        return double.Parse(_smallScientificNumber, CultureInfo.InvariantCulture);
    }

    [BenchmarkCategory("Parse")]
    [Benchmark]
    public double Parse_FastDouble_SmallScientific()
    {
        return FastDouble.ParseDouble(_smallScientificNumber);
    }

    [BenchmarkCategory("Parse")]
    [Benchmark]
    public double Parse_DoubleParse_LongScientific()
    {
        return double.Parse(_longScientificNumber, CultureInfo.InvariantCulture);
    }

    [BenchmarkCategory("Parse")]
    [Benchmark]
    public double Parse_FastDouble_LongScientific()
    {
        return FastDouble.ParseDouble(_longScientificNumber);
    }

    [BenchmarkCategory("Parse")]
    [Benchmark]
    public double Parse_DoubleParse_Negative()
    {
        return double.Parse(_negativeNumber, CultureInfo.InvariantCulture);
    }

    [BenchmarkCategory("Parse")]
    [Benchmark]
    public double Parse_FastDouble_Negative()
    {
        return FastDouble.ParseDouble(_negativeNumber);
    }

    [BenchmarkCategory("Parse")]
    [Benchmark]
    public double Parse_DoubleParse_SpanSlice()
    {
        return double.Parse(_sliceSource.AsSpan(6), CultureInfo.InvariantCulture);
    }

    [BenchmarkCategory("Parse")]
    [Benchmark]
    public double Parse_FastDouble_SpanSlice()
    {
        return FastDouble.ParseDouble(_sliceSource.AsSpan(6));
    }

    #endregion

    #region OptimizeToString vs double.ToString

    [BenchmarkCategory("ToString")]
    [Benchmark]
    public string ToString_DoubleToString_ZeroDecimals()
    {
        return _mediumDouble.ToString("F0", CultureInfo.InvariantCulture);
    }

    [BenchmarkCategory("ToString")]
    [Benchmark]
    public string ToString_FastDouble_ZeroDecimals()
    {
        return _mediumDouble.OptimizeToString(0);
    }

    [BenchmarkCategory("ToString")]
    [Benchmark]
    public string ToString_DoubleToString_Small()
    {
        return _smallDouble.ToString("F2", CultureInfo.InvariantCulture);
    }

    [BenchmarkCategory("ToString")]
    [Benchmark]
    public string ToString_FastDouble_Small()
    {
        return _smallDouble.OptimizeToString(2);
    }

    [BenchmarkCategory("ToString")]
    [Benchmark]
    public string ToString_DoubleToString_Medium()
    {
        return _mediumDouble.ToString("F3", CultureInfo.InvariantCulture);
    }

    [BenchmarkCategory("ToString")]
    [Benchmark]
    public string ToString_FastDouble_Medium()
    {
        return _mediumDouble.OptimizeToString(3);
    }

    [BenchmarkCategory("ToString")]
    [Benchmark]
    public string ToString_DoubleToString_Large()
    {
        return _largeDouble.ToString("F2", CultureInfo.InvariantCulture);
    }

    [BenchmarkCategory("ToString")]
    [Benchmark]
    public string ToString_FastDouble_Large()
    {
        return _largeDouble.OptimizeToString(2);
    }

    [BenchmarkCategory("ToString")]
    [Benchmark]
    public string ToString_DoubleToString_RoundingCarry()
    {
        return _roundingCarryDouble.ToString("F2", CultureInfo.InvariantCulture);
    }

    [BenchmarkCategory("ToString")]
    [Benchmark]
    public string ToString_FastDouble_RoundingCarry()
    {
        return _roundingCarryDouble.OptimizeToString(2);
    }

    #endregion

    #region Precision Variations

    [BenchmarkCategory("Precision")]
    [Benchmark]
    public double Parse_FastDouble_Precision3()
    {
        return FastDouble.ParseDouble(_decimalNumber, 3);
    }

    [BenchmarkCategory("Precision")]
    [Benchmark]
    public double Parse_FastDouble_Precision6()
    {
        return FastDouble.ParseDouble(_decimalNumber, 6);
    }

    [BenchmarkCategory("Precision")]
    [Benchmark]
    public double Parse_FastDouble_Precision10()
    {
        return FastDouble.ParseDouble(_decimalNumber, 10);
    }

    #endregion
}
