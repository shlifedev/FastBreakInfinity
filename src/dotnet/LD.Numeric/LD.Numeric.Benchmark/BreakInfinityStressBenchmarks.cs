using System.Globalization;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using FastBigDouble = LD.Numeric.IdleNumber.BigDouble;
using FastBigMath = LD.Numeric.IdleNumber.BigMath;
using OriginalBigDouble = BreakInfinity.BigDouble;
using OriginalBigMath = BreakInfinity.BigMath;

namespace LD.Numeric.Benchmark;

[MemoryDiagnoser]
[RankColumn]
public class BreakInfinityStressBenchmarks
{
    private const int OperationCount = 10_000;
    private const int DataSetSize = 1024;
    private const int DataMask = DataSetSize - 1;
    private const int OwnedSetSize = 32;
    private const int OwnedMask = OwnedSetSize - 1;

    private readonly CultureInfo _culture = CultureInfo.InvariantCulture;
    private string[] _scientificValues = null!;

    private FastBigDouble[] _fastPrices = null!;
    private FastBigDouble[] _fastOwned = null!;
    private FastBigDouble[] _fastMultipliers = null!;
    private FastBigDouble _fastInitialWallet;
    private FastBigDouble _fastBaseCost;
    private FastBigDouble _fastPriceRatio;

    private OriginalBigDouble[] _originalPrices = null!;
    private OriginalBigDouble[] _originalOwned = null!;
    private OriginalBigDouble[] _originalMultipliers = null!;
    private OriginalBigDouble _originalInitialWallet;
    private OriginalBigDouble _originalBaseCost;
    private OriginalBigDouble _originalPriceRatio;

    [GlobalSetup]
    public void Setup()
    {
        _scientificValues = new string[DataSetSize];
        _fastPrices = new FastBigDouble[DataSetSize];
        _originalPrices = new OriginalBigDouble[DataSetSize];
        _fastOwned = new FastBigDouble[OwnedSetSize];
        _originalOwned = new OriginalBigDouble[OwnedSetSize];
        _fastMultipliers = new FastBigDouble[8];
        _originalMultipliers = new OriginalBigDouble[8];

        for (int i = 0; i < DataSetSize; i++)
        {
            double mantissa = 1.000001 + ((i * 3719) % 8_900_000) / 1_000_000.0;
            long exponent = 30 + ((long)i * 7919) % 1_000_000_000L;
            _scientificValues[i] =
                mantissa.ToString("0.000000", _culture) + "e" + exponent.ToString(_culture);

            double priceMantissa = 1.100001 + ((i * 1543) % 8_500_000) / 1_000_000.0;
            long priceExponent = exponent + (i % 17) - 8;
            _fastPrices[i] = new FastBigDouble(priceMantissa, priceExponent);
            _originalPrices[i] = new OriginalBigDouble(priceMantissa, priceExponent);
        }

        for (int i = 0; i < OwnedSetSize; i++)
        {
            _fastOwned[i] = new FastBigDouble(100 + i * 37);
            _originalOwned[i] = new OriginalBigDouble(100 + i * 37);
        }

        for (int i = 0; i < _fastMultipliers.Length; i++)
        {
            double multiplier = 1.0001 + i * 0.00003;
            _fastMultipliers[i] = new FastBigDouble(multiplier);
            _originalMultipliers[i] = new OriginalBigDouble(multiplier);
        }

        _fastInitialWallet = new FastBigDouble(9.876543, 999_950_000);
        _originalInitialWallet = new OriginalBigDouble(9.876543, 999_950_000);
        _fastBaseCost = new FastBigDouble(1.25, 30);
        _originalBaseCost = new OriginalBigDouble(1.25, 30);
        _fastPriceRatio = new FastBigDouble(1.0005);
        _originalPriceRatio = new OriginalBigDouble(1.0005);
    }

    [Benchmark(Baseline = true, OperationsPerInvoke = OperationCount)]
    public long BreakInfinityCs_Stress()
    {
        var wallet = _originalInitialWallet;
        long checksum = 0;

        for (int i = 0; i < OperationCount; i++)
        {
            int dataIndex = i & DataMask;
            var income = OriginalBigDouble.Parse(_scientificValues[dataIndex]);
            wallet += income;
            wallet *= _originalMultipliers[i & 7];

            var price = _originalPrices[dataIndex];
            if (wallet >= price)
            {
                wallet -= price;
            }

            var affordable = OriginalBigMath.AffordGeometricSeries(
                wallet,
                _originalBaseCost,
                _originalPriceRatio,
                _originalOwned[i & OwnedMask]
            );
            string display = wallet.ToString();
            checksum += display.Length + (wallet.Exponent & 0xFF) + (affordable.Exponent & 0xFF);
        }

        return checksum;
    }

    [Benchmark(OperationsPerInvoke = OperationCount)]
    public long FastBreakInfinity_Stress()
    {
        var wallet = _fastInitialWallet;
        long checksum = 0;

        for (int i = 0; i < OperationCount; i++)
        {
            int dataIndex = i & DataMask;
            var income = FastBigDouble.Parse(_scientificValues[dataIndex]);
            wallet += income;
            wallet *= _fastMultipliers[i & 7];

            var price = _fastPrices[dataIndex];
            if (wallet >= price)
            {
                wallet -= price;
            }

            var affordable = FastBigMath.AffordGeometricSeries(
                wallet,
                _fastBaseCost,
                _fastPriceRatio,
                _fastOwned[i & OwnedMask]
            );
            string display = wallet.ToString();
            checksum += display.Length + (wallet.Exponent & 0xFF) + (affordable.Exponent & 0xFF);
        }

        return checksum;
    }
}
