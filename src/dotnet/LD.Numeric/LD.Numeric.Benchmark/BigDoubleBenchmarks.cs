using BenchmarkDotNet.Attributes;
using LD.Numeric.IdleNumber;

namespace LD.Numeric.Benchmark;

[MemoryDiagnoser]
public class BigDoubleBenchmarks
{
    private BigDouble _small;
    private BigDouble _medium;
    private BigDouble _large;
    private BigDouble _veryLarge;
    private BigDouble _roundingCarry;

    private string _plainString = null!;
    private string _mediumString = null!;
    private string _longExponentString = null!;

    [GlobalSetup]
    public void Setup()
    {
        _small = new BigDouble(123.456);
        _medium = new BigDouble(1.5, 10);
        _large = new BigDouble(9.999, 100);
        _veryLarge = new BigDouble(5.5, 1000);
        _roundingCarry = new BigDouble("9.999999e23");

        _plainString = "123456789.123456";
        _mediumString = "1.5e10";
        _longExponentString = "1.0000000000001e999999999";
    }

    #region Constructors

    [BenchmarkCategory("Constructor")]
    [Benchmark]
    public BigDouble Constructor_FromDouble()
    {
        return new BigDouble(123456.789);
    }

    [BenchmarkCategory("Constructor")]
    [Benchmark]
    public BigDouble Constructor_FromString()
    {
        return BigDouble.Parse(_mediumString);
    }

    [BenchmarkCategory("Constructor")]
    [Benchmark]
    public BigDouble Constructor_FromPlainString()
    {
        return BigDouble.Parse(_plainString);
    }

    [BenchmarkCategory("Constructor")]
    [Benchmark]
    public BigDouble Constructor_FromLongExponentString()
    {
        return BigDouble.Parse(_longExponentString);
    }

    [BenchmarkCategory("Constructor")]
    [Benchmark]
    public BigDouble Constructor_FromMantissaExponent()
    {
        return new BigDouble(1.5, 100);
    }

    #endregion

    #region Arithmetic

    [BenchmarkCategory("Arithmetic")]
    [Benchmark]
    public BigDouble Arithmetic_Add_SameExponent()
    {
        return _medium + _medium;
    }

    [BenchmarkCategory("Arithmetic")]
    [Benchmark]
    public BigDouble Arithmetic_Add_DifferentExponent()
    {
        return _small + _large;
    }

    [BenchmarkCategory("Arithmetic")]
    [Benchmark]
    public BigDouble Arithmetic_Subtract()
    {
        return _large - _medium;
    }

    [BenchmarkCategory("Arithmetic")]
    [Benchmark]
    public BigDouble Arithmetic_Multiply()
    {
        return _medium * _large;
    }

    [BenchmarkCategory("Arithmetic")]
    [Benchmark]
    public BigDouble Arithmetic_Divide()
    {
        return _large / _medium;
    }

    [BenchmarkCategory("Arithmetic")]
    [Benchmark]
    public BigDouble Arithmetic_Reciprocate()
    {
        return BigDouble.Reciprocate(_medium);
    }

    #endregion

    #region MathFunctions

    [BenchmarkCategory("Math")]
    [Benchmark]
    public BigDouble Math_Pow_SmallExponent()
    {
        return BigDouble.Pow(_medium, 5);
    }

    [BenchmarkCategory("Math")]
    [Benchmark]
    public BigDouble Math_Pow_LargeExponent()
    {
        return BigDouble.Pow(_medium, 100);
    }

    [BenchmarkCategory("Math")]
    [Benchmark]
    public BigDouble Math_Pow_DoubleExponent()
    {
        return BigDouble.Pow(_medium, 2.5);
    }

    [BenchmarkCategory("Math")]
    [Benchmark]
    public BigDouble Math_Pow10_Integer()
    {
        return BigDouble.Pow10(100L);
    }

    [BenchmarkCategory("Math")]
    [Benchmark]
    public BigDouble Math_Pow10_Double()
    {
        return BigDouble.Pow10(100.5);
    }

    [BenchmarkCategory("Math")]
    [Benchmark]
    public double Math_Log10()
    {
        return BigDouble.Log10(_large);
    }

    [BenchmarkCategory("Math")]
    [Benchmark]
    public double Math_Log_Base2()
    {
        return BigDouble.Log(_large, 2);
    }

    [BenchmarkCategory("Math")]
    [Benchmark]
    public double Math_Log2()
    {
        return BigDouble.Log2(_large);
    }

    [BenchmarkCategory("Math")]
    [Benchmark]
    public double Math_Ln()
    {
        return BigDouble.Ln(_large);
    }

    [BenchmarkCategory("Math")]
    [Benchmark]
    public BigDouble Math_Sqrt()
    {
        return BigDouble.Sqrt(_large);
    }

    [BenchmarkCategory("Math")]
    [Benchmark]
    public BigDouble Math_Cbrt()
    {
        return BigDouble.Cbrt(_large);
    }

    [BenchmarkCategory("Math")]
    [Benchmark]
    public BigDouble Math_Exp()
    {
        return BigDouble.Exp(_small);
    }

    [BenchmarkCategory("Math")]
    [Benchmark]
    public BigDouble Math_Sinh()
    {
        return BigDouble.Sinh(_small);
    }

    [BenchmarkCategory("Math")]
    [Benchmark]
    public BigDouble Math_Cosh()
    {
        return BigDouble.Cosh(_small);
    }

    [BenchmarkCategory("Math")]
    [Benchmark]
    public BigDouble Math_Tanh()
    {
        return BigDouble.Tanh(_small);
    }

    [BenchmarkCategory("Math")]
    [Benchmark]
    public BigDouble Math_Factorial()
    {
        return BigDouble.Factorial(100);
    }

    [BenchmarkCategory("Math")]
    [Benchmark]
    public BigDouble Math_Abs()
    {
        return BigDouble.Abs(-_medium);
    }

    #endregion

    #region Rounding

    [BenchmarkCategory("Rounding")]
    [Benchmark]
    public BigDouble Rounding_Round()
    {
        return BigDouble.Round(_small);
    }

    [BenchmarkCategory("Rounding")]
    [Benchmark]
    public BigDouble Rounding_Floor()
    {
        return BigDouble.Floor(_small);
    }

    [BenchmarkCategory("Rounding")]
    [Benchmark]
    public BigDouble Rounding_Ceiling()
    {
        return BigDouble.Ceiling(_small);
    }

    [BenchmarkCategory("Rounding")]
    [Benchmark]
    public BigDouble Rounding_Truncate()
    {
        return BigDouble.Truncate(_small);
    }

    #endregion

    #region Comparison

    [BenchmarkCategory("Comparison")]
    [Benchmark]
    public bool Comparison_Equals()
    {
        return _medium == _large;
    }

    [BenchmarkCategory("Comparison")]
    [Benchmark]
    public bool Comparison_Equals_Tolerance()
    {
        return _medium.Equals(_large, 1e-9);
    }

    [BenchmarkCategory("Comparison")]
    [Benchmark]
    public int Comparison_GetHashCode()
    {
        return _medium.GetHashCode();
    }

    [BenchmarkCategory("Comparison")]
    [Benchmark]
    public bool Comparison_LessThan()
    {
        return _medium < _large;
    }

    [BenchmarkCategory("Comparison")]
    [Benchmark]
    public bool Comparison_GreaterThan()
    {
        return _large > _medium;
    }

    [BenchmarkCategory("Comparison")]
    [Benchmark]
    public int Comparison_CompareTo()
    {
        return _medium.CompareTo(_large);
    }

    [BenchmarkCategory("Comparison")]
    [Benchmark]
    public BigDouble Comparison_Max()
    {
        return BigDouble.Max(_medium, _large);
    }

    [BenchmarkCategory("Comparison")]
    [Benchmark]
    public BigDouble Comparison_Min()
    {
        return BigDouble.Min(_medium, _large);
    }

    #endregion

    #region Conversion

    [BenchmarkCategory("Conversion")]
    [Benchmark]
    public string Conversion_ToString_Small()
    {
        return _small.ToString();
    }

    [BenchmarkCategory("Conversion")]
    [Benchmark]
    public string Conversion_ToString_Large()
    {
        return _large.ToString();
    }

    [BenchmarkCategory("Conversion")]
    [Benchmark]
    public string Conversion_ToString_VeryLarge()
    {
        return _veryLarge.ToString();
    }

    [BenchmarkCategory("Conversion")]
    [Benchmark]
    public string Conversion_ToString_RoundingCarry()
    {
        return _roundingCarry.ToString();
    }

    [BenchmarkCategory("Conversion")]
    [Benchmark]
    public string Conversion_ToStringMantissaExponent()
    {
        return _veryLarge.ToStringMantissaExponent();
    }

    [BenchmarkCategory("Conversion")]
    [Benchmark]
    public double Conversion_ToDouble()
    {
        return _medium.ToDouble();
    }

    [BenchmarkCategory("Conversion")]
    [Benchmark]
    public float Conversion_ToFloat()
    {
        return _medium.ToFloat();
    }

    [BenchmarkCategory("Conversion")]
    [Benchmark]
    public double Conversion_AdjustedMantissa()
    {
        return _large.AdjustedMantissa();
    }

    [BenchmarkCategory("Conversion")]
    [Benchmark]
    public string Conversion_GetAlphabetUnit()
    {
        return BigDouble.GetAlphabetUnit(100);
    }

    [BenchmarkCategory("Conversion")]
    [Benchmark]
    public string Conversion_GetAlphabetUnit_Large()
    {
        return BigDouble.GetAlphabetUnit(1000);
    }

    [BenchmarkCategory("Conversion")]
    [Benchmark]
    public (long rangeA, long rangeB) Conversion_GetExponentFromAlphabetUnit()
    {
        return BigDouble.GetExponentFromAlphabetUnit("AA");
    }

    [BenchmarkCategory("Conversion")]
    [Benchmark]
    public long Conversion_GetNumberFromUnitName()
    {
        return BigDouble.GetNumberFromUnitName("AA");
    }

    [BenchmarkCategory("Conversion")]
    [Benchmark]
    public long Conversion_GetExponentFromUnitName()
    {
        return BigDouble.GetExponentFromUnitName("AA");
    }

    #endregion
}
