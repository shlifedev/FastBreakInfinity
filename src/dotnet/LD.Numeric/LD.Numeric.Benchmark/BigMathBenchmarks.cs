using BenchmarkDotNet.Attributes;
using LD.Numeric.IdleNumber;

namespace LD.Numeric.Benchmark;

[MemoryDiagnoser]
public class BigMathBenchmarks
{
    private BigDouble _resourcesAvailable;
    private BigDouble _priceStart;
    private BigDouble _priceRatio;
    private BigDouble _priceRatioOne;
    private BigDouble _priceAdd;
    private BigDouble _currentOwned;
    private BigDouble _numItems;
    private BigDouble _currentRpS;
    private BigDouble _deltaRpS;

    [GlobalSetup]
    public void Setup()
    {
        _resourcesAvailable = new BigDouble(1.25, 120);
        _priceStart = new BigDouble(1.5, 12);
        _priceRatio = new BigDouble(1.07);
        _priceRatioOne = BigDouble.One;
        _priceAdd = new BigDouble(2.5, 10);
        _currentOwned = new BigDouble(250);
        _numItems = new BigDouble(100);
        _currentRpS = new BigDouble(5.5, 30);
        _deltaRpS = new BigDouble(2.5, 25);
    }

    #region GeometricSeries

    [BenchmarkCategory("BigMath")]
    [Benchmark]
    public BigDouble BigMath_AffordGeometricSeries()
    {
        return BigMath.AffordGeometricSeries(
            _resourcesAvailable,
            _priceStart,
            _priceRatio,
            _currentOwned
        );
    }

    [BenchmarkCategory("BigMath")]
    [Benchmark]
    public BigDouble BigMath_AffordGeometricSeries_FixedPrice()
    {
        return BigMath.AffordGeometricSeries(
            _resourcesAvailable,
            _priceStart,
            _priceRatioOne,
            _currentOwned
        );
    }

    [BenchmarkCategory("BigMath")]
    [Benchmark]
    public BigDouble BigMath_SumGeometricSeries()
    {
        return BigMath.SumGeometricSeries(_numItems, _priceStart, _priceRatio, _currentOwned);
    }

    [BenchmarkCategory("BigMath")]
    [Benchmark]
    public BigDouble BigMath_SumGeometricSeries_FixedPrice()
    {
        return BigMath.SumGeometricSeries(_numItems, _priceStart, _priceRatioOne, _currentOwned);
    }

    #endregion

    #region ArithmeticSeries

    [BenchmarkCategory("BigMath")]
    [Benchmark]
    public BigDouble BigMath_AffordArithmeticSeries()
    {
        return BigMath.AffordArithmeticSeries(
            _resourcesAvailable,
            _priceStart,
            _priceAdd,
            _currentOwned
        );
    }

    [BenchmarkCategory("BigMath")]
    [Benchmark]
    public BigDouble BigMath_SumArithmeticSeries()
    {
        return BigMath.SumArithmeticSeries(_numItems, _priceStart, _priceAdd, _currentOwned);
    }

    #endregion

    #region PurchaseUtility

    [BenchmarkCategory("BigMath")]
    [Benchmark]
    public BigDouble BigMath_EfficiencyOfPurchase()
    {
        return BigMath.EfficiencyOfPurchase(_priceStart, _currentRpS, _deltaRpS);
    }

    [BenchmarkCategory("BigMath")]
    [Benchmark]
    public BigDouble BigMath_RandomBigDouble()
    {
        return BigMath.RandomBigDouble(1000);
    }

    #endregion
}
