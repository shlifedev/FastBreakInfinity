using LD.Numeric.IdleNumber;

namespace LD.Numeric.Test;

public class ExtendedMathTests
{
    [Test]
    public void Abs_NegativeAndPositive()
    {
        Assert.That(BigDouble.Abs(new BigDouble(-5, 100)) == new BigDouble(5, 100), Is.True);
        Assert.That(BigDouble.Abs(new BigDouble(5, 100)) == new BigDouble(5, 100), Is.True);
        Assert.That(BigDouble.Abs(BigDouble.Zero) == BigDouble.Zero, Is.True);
    }

    [Test]
    public void Negate_FlipsSign()
    {
        Assert.That(BigDouble.Negate(new BigDouble(5, 100)) == new BigDouble(-5, 100), Is.True);
        Assert.That(-new BigDouble(-3) == new BigDouble(3), Is.True);
    }

    [Test]
    public void Sign_ReturnsCorrectSign()
    {
        Assert.That(BigDouble.Sign(new BigDouble(5, 100)), Is.EqualTo(1));
        Assert.That(BigDouble.Sign(new BigDouble(-5, 100)), Is.EqualTo(-1));
        Assert.That(BigDouble.Sign(BigDouble.Zero), Is.EqualTo(0));
    }

    [Test]
    public void Reciprocate_Basic()
    {
        Assert.That(BigDouble.Reciprocate(2).Equals(new BigDouble(0.5), 1e-12), Is.True);
        Assert.That(
            BigDouble.Reciprocate(new BigDouble(4, 2)).Equals(new BigDouble(2.5, -3), 1e-12),
            Is.True
        );
    }

    [Test]
    public void Reciprocate_HugeExponent()
    {
        var result = BigDouble.Reciprocate(new BigDouble(2, 1000));
        Assert.That(
            result.Equals(new BigDouble(5, -1001), 1e-12),
            Is.True,
            $"실제 {result.ToStringMantissaExponent()}"
        );
    }

    [Test]
    public void IncrementDecrement_Operators()
    {
        BigDouble a = 5;
        a++;
        Assert.That(a == new BigDouble(6), Is.True);
        a--;
        a--;
        Assert.That(a == new BigDouble(4), Is.True);
    }

    [Test]
    public void Pow_IntegerPower_Exact()
    {
        Assert.That(BigDouble.Pow(2, 10) == new BigDouble(1024), Is.True);
        Assert.That(BigDouble.Pow(new BigDouble(1, 100), 2) == new BigDouble(1, 200), Is.True);
    }

    [Test]
    public void Pow_NegativeBase_IntegerPower()
    {
        Assert.That(BigDouble.Pow(-2, 3.0).Equals(new BigDouble(-8), 1e-12), Is.True);
        Assert.That(BigDouble.Pow(-2, 2.0).Equals(new BigDouble(4), 1e-12), Is.True);
    }

    [Test]
    public void Pow_NegativeBase_FractionalPower_ReturnsNaN()
    {
        Assert.That(BigDouble.IsNaN(BigDouble.Pow(-4, 0.5)), Is.True);
    }

    [Test]
    public void Pow_FractionalPower()
    {
        Assert.That(BigDouble.Pow(10, 0.5).Equals(BigDouble.Sqrt(10), 1e-12), Is.True);
        Assert.That(
            BigDouble.Pow(new BigDouble(1, 100), 0.5).Equals(new BigDouble(1, 50), 1e-9),
            Is.True
        );
    }

    [Test]
    public void Pow_ZeroPower_ReturnsOne()
    {
        Assert.That(BigDouble.Pow(new BigDouble(7, 77), 0L) == BigDouble.One, Is.True);
    }

    [Test]
    public void Pow_ExponentOverflow_ReturnsInfinityOrZero()
    {
        var huge = BigDouble.Pow10(5_000_000_000_000_000_000L);
        Assert.That(BigDouble.IsPositiveInfinity(BigDouble.Pow(huge, 10L)), Is.True);

        var tiny = BigDouble.Pow10(-5_000_000_000_000_000_000L);
        Assert.That(BigDouble.Pow(tiny, 10L) == BigDouble.Zero, Is.True);
    }

    [Test]
    public void Multiply_ExponentOverflow_ReturnsInfinityOrZero()
    {
        var huge = BigDouble.Pow10(9_000_000_000_000_000_000L);
        Assert.That(BigDouble.IsPositiveInfinity(huge * huge), Is.True);

        var tiny = BigDouble.Pow10(-9_000_000_000_000_000_000L);
        Assert.That(tiny * tiny == BigDouble.Zero, Is.True);
    }

    [Test]
    public void Pow10_FractionalPower()
    {
        Assert.That(BigDouble.Pow10(0.5).Equals(BigDouble.Sqrt(10), 1e-12), Is.True);
        Assert.That(BigDouble.Pow10(100.0) == new BigDouble(1, 100), Is.True);
    }

    [Test]
    public void Sqrt_NegativeValue_ReturnsNaN()
    {
        Assert.That(BigDouble.IsNaN(BigDouble.Sqrt(-4)), Is.True);
    }

    [Test]
    public void Sqrt_HugeValue_OddExponent()
    {
        var result = BigDouble.Sqrt(new BigDouble(1, 101));
        Assert.That(result.Equals(new BigDouble(3.16227766016838, 50), 1e-9), Is.True);
    }

    [Test]
    public void Cbrt_Basic()
    {
        Assert.That(BigDouble.Cbrt(27).Equals(new BigDouble(3), 1e-12), Is.True);
        Assert.That(BigDouble.Cbrt(-27).Equals(new BigDouble(-3), 1e-12), Is.True);
        Assert.That(BigDouble.Cbrt(new BigDouble(1, 3)).Equals(new BigDouble(10), 1e-12), Is.True);
    }

    [Test]
    public void Cbrt_HugeValue_ExponentModBranches()
    {
        Assert.That(
            BigDouble.Log10(BigDouble.Cbrt(new BigDouble(1, 99))),
            Is.EqualTo(33.0).Within(1e-9)
        );
        Assert.That(
            BigDouble.Log10(BigDouble.Cbrt(new BigDouble(1, 100))),
            Is.EqualTo(100 / 3.0).Within(1e-9)
        );
        Assert.That(
            BigDouble.Log10(BigDouble.Cbrt(new BigDouble(1, 101))),
            Is.EqualTo(101 / 3.0).Within(1e-9)
        );
    }

    [Test]
    public void Exp_Basic()
    {
        Assert.That(BigDouble.Exp(1).Equals(new BigDouble(Math.E), 1e-9), Is.True);
        Assert.That(BigDouble.Exp(0).Equals(BigDouble.One, 1e-9), Is.True);
    }

    [Test]
    public void Exp_LargeValue_MatchesLog()
    {
        var result = BigDouble.Exp(1000);
        Assert.That(BigDouble.Ln(result), Is.EqualTo(1000.0).Within(1e-6));
    }

    [Test]
    public void Factorial_SmallValues_NearExact()
    {
        Assert.That(BigDouble.Factorial(5).Equals(new BigDouble(120), 1e-6), Is.True);
        Assert.That(BigDouble.Factorial(10).Equals(new BigDouble(3628800), 1e-6), Is.True);
    }

    [Test]
    public void Factorial_LargeValue_MatchesStirlingMagnitude()
    {
        var result = BigDouble.Factorial(100);
        Assert.That(BigDouble.Log10(result), Is.EqualTo(157.97).Within(0.01));
    }

    [Test]
    public void Factorial_ResultExponentOverflow_ReturnsPositiveInfinity()
    {
        var result = BigDouble.Factorial(new BigDouble(1, 30));
        Assert.That(BigDouble.IsPositiveInfinity(result), Is.True, $"실제 {result}");
    }

    [Test]
    public void Factorial_BeyondDoubleRange_ReturnsPositiveInfinity()
    {
        var result = BigDouble.Factorial(new BigDouble(1, 400));
        Assert.That(BigDouble.IsPositiveInfinity(result), Is.True, $"실제 {result}");
    }

    [Test]
    public void Hyperbolic_BasicValues()
    {
        Assert.That(BigDouble.Sinh(1).Equals(new BigDouble(Math.Sinh(1)), 1e-9), Is.True);
        Assert.That(BigDouble.Cosh(1).Equals(new BigDouble(Math.Cosh(1)), 1e-9), Is.True);
        Assert.That(BigDouble.Tanh(1).Equals(new BigDouble(Math.Tanh(1)), 1e-9), Is.True);
    }

    [Test]
    public void InverseHyperbolic_BasicValues()
    {
        Assert.That(BigDouble.Asinh(1), Is.EqualTo(Math.Asinh(1)).Within(1e-9));
        Assert.That(BigDouble.Acosh(2), Is.EqualTo(Math.Acosh(2)).Within(1e-9));
        Assert.That(BigDouble.Atanh(0.5), Is.EqualTo(Math.Atanh(0.5)).Within(1e-9));
    }

    [Test]
    public void Atanh_OutOfDomain_ReturnsNaN()
    {
        Assert.That(double.IsNaN(BigDouble.Atanh(1)), Is.True);
        Assert.That(double.IsNaN(BigDouble.Atanh(-2)), Is.True);
    }

    [Test]
    public void Log10_OutOfDomain_FollowsIeeeSemantics()
    {
        Assert.That(double.IsNaN(BigDouble.Log10(new BigDouble(-5))), Is.True);
        Assert.That(double.IsNegativeInfinity(BigDouble.Log10(BigDouble.Zero)), Is.True);
    }

    [Test]
    public void Log10_HugeValue()
    {
        Assert.That(BigDouble.Log10(new BigDouble(1, 100)), Is.EqualTo(100.0).Within(1e-12));
        Assert.That(
            BigDouble.Log10(new BigDouble(5, 100)),
            Is.EqualTo(100.0 + Math.Log10(5)).Within(1e-12)
        );
    }

    [Test]
    public void AbsLog10_NegativeValue()
    {
        Assert.That(BigDouble.AbsLog10(new BigDouble(-1, 50)), Is.EqualTo(50.0).Within(1e-12));
    }

    [Test]
    public void Log2_And_Ln()
    {
        Assert.That(BigDouble.Log2(8), Is.EqualTo(3.0).Within(1e-9));
        Assert.That(BigDouble.Ln(new BigDouble(Math.E)), Is.EqualTo(1.0).Within(1e-9));
    }

    [Test]
    public void Log_ArbitraryBase()
    {
        Assert.That(BigDouble.Log(100, 10.0), Is.EqualTo(2.0).Within(1e-9));
        Assert.That(BigDouble.Log(8, 2.0), Is.EqualTo(3.0).Within(1e-9));
        Assert.That(
            BigDouble.Log(new BigDouble(1, 100), new BigDouble(10)),
            Is.EqualTo(100.0).Within(1e-9)
        );
    }

    [Test]
    public void Log_BaseZero_ReturnsNaN()
    {
        Assert.That(double.IsNaN(BigDouble.Log(5, 0.0)), Is.True);
    }

    [Test]
    public void MinMax_WithNaN_ReturnsNaN()
    {
        Assert.That(BigDouble.IsNaN(BigDouble.Max(BigDouble.NaN, 5)), Is.True);
        Assert.That(BigDouble.IsNaN(BigDouble.Max(5, BigDouble.NaN)), Is.True);
        Assert.That(BigDouble.IsNaN(BigDouble.Min(BigDouble.NaN, 5)), Is.True);
        Assert.That(BigDouble.IsNaN(BigDouble.Min(5, BigDouble.NaN)), Is.True);
    }

    [Test]
    public void ExtensionMethods_MatchStaticMethods()
    {
        BigDouble value = new BigDouble(2, 50);
        Assert.That(value.Sqr() == BigDouble.Pow(value, 2L), Is.True);
        Assert.That(value.Abs() == BigDouble.Abs(value), Is.True);
        Assert.That(value.Sqrt() == BigDouble.Sqrt(value), Is.True);
        Assert.That(value.Reciprocate() == BigDouble.Reciprocate(value), Is.True);
        Assert.That(value.Sign(), Is.EqualTo(1));
    }

    [Test]
    public void PowerRange_NegativePowerKeepsTinyMantissaResult()
    {
        Assert.That(BigDouble.Pow(2, -2000L).Exponent, Is.EqualTo(-603));
        Assert.That(BigDouble.Pow(2, -2000.0).Mantissa, Is.EqualTo(8.7098098162172).Within(1e-11));
        Assert.That(BigDouble.Pow(-2, -2001L).Mantissa, Is.EqualTo(-4.3549049081086).Within(1e-11));
    }

    [Test]
    public void PowerRange_IntegerPowerRetainsParityAndExponent()
    {
        Assert.That(BigDouble.Pow(-1, long.MaxValue), Is.EqualTo(-BigDouble.One));
        var value = new BigDouble(1, 9007199254740993L);
        Assert.That(BigDouble.Pow(value, 1.0), Is.EqualTo(value));
        Assert.That(BigDouble.Pow(value, 2.0).Exponent, Is.EqualTo(18014398509481986L));
    }

    [TestCase(1e20)]
    [TestCase(double.PositiveInfinity)]
    public void PowerRange_Pow10SaturatesAtExponentLimits(double power)
    {
        Assert.That(BigDouble.IsPositiveInfinity(BigDouble.Pow10(power)), Is.True);
        Assert.That(BigDouble.Pow10(-power), Is.EqualTo(BigDouble.Zero));
        Assert.That(BigDouble.IsPositiveInfinity(BigDouble.Pow(2, power)), Is.True);
        Assert.That(BigDouble.Pow(2, -power), Is.EqualTo(BigDouble.Zero));
    }

    [Test]
    public void LogRange_ExtendedBasesKeepTheirExponent()
    {
        Assert.That(BigDouble.Log(new BigDouble(1, 800), new BigDouble(1, 400)), Is.EqualTo(2));
        Assert.That(BigDouble.Log(new BigDouble(1, 800), new BigDouble(1, -400)), Is.EqualTo(-2));
    }

    [Test]
    public void PowerRange_IntegralPowersOfSpecialValues()
    {
        Assert.That(BigDouble.IsPositiveInfinity(BigDouble.Pow(BigDouble.Zero, -1L)), Is.True);
        Assert.That(
            BigDouble.IsNegativeInfinity(BigDouble.Pow(BigDouble.NegativeInfinity, 3L)),
            Is.True
        );
        Assert.That(BigDouble.Pow(BigDouble.PositiveInfinity, -2L), Is.EqualTo(BigDouble.Zero));
        Assert.That(BigDouble.IsNaN(BigDouble.Pow(BigDouble.NaN, 3.0)), Is.True);
        Assert.That(BigDouble.Pow(BigDouble.NaN, 0.0), Is.EqualTo(BigDouble.One));
    }

    [Test]
    public void PowerRange_FractionalExponentKeepsMantissaAtLargeScale()
    {
        var result = BigDouble.Pow(new BigDouble(4, 1000000000000000000L), 0.5);
        Assert.That(result.Mantissa, Is.EqualTo(2));
        Assert.That(result.Exponent, Is.EqualTo(500000000000000000L));
    }

    [TestCase(9007199254740993L)]
    [TestCase(long.MaxValue)]
    public void WideFractionalPowerUsesExactOddExponent(long exponent)
    {
        var result = BigDouble.Pow(new BigDouble(4, exponent), 0.5);
        Assert.That(result.Mantissa, Is.EqualTo(Math.Sqrt(40)).Within(2e-14));
        Assert.That(result.Exponent, Is.EqualTo(exponent / 2));
    }

    [TestCase(9.999999999999998)]
    [TestCase(9.99999999999999)]
    public void NearOneLogResolvesSmallDistancesBelowOne(double mantissa)
    {
        var value = new BigDouble(mantissa, -1);
        var delta = (mantissa - 10) / 10;
        double ln = delta * (1 - delta / 2);
        Assert.That(BigDouble.Log(2, value), Is.EqualTo(Math.Log(2) / ln).Within(1e-13).Percent);
        Assert.That(BigDouble.Log10(value), Is.EqualTo(ln / Math.Log(10)).Within(1e-13).Percent);
    }

    [Test]
    public void NearOneIntegerPowersRemainReciprocal()
    {
        var value = new BigDouble(9.999999999999998, -1);
        const long power = 10000000000000000;
        var inverse = BigDouble.Pow(value, -power);
        var forward = BigDouble.Pow(value, power);
        Assert.That(inverse.ToDouble(), Is.EqualTo(5.90829230394221).Within(1e-11));
        Assert.That((inverse * forward).ToDouble(), Is.EqualTo(1).Within(2e-14));
    }

    [Test]
    public void NearOneLogAboveOneKeepsBasePrecision()
    {
        var value = new BigDouble(Math.BitIncrement(1.0));
        Assert.That(
            BigDouble.Log(2, value),
            Is.EqualTo(Math.Log(2) / Math.Log(value.Mantissa)).Within(1e-13).Percent
        );
    }

    [TestCase(0.1, 2.2387211385683396, 900719925474099L)]
    [TestCase(-0.1, 4.466835921509631, -900719925474100L)]
    public void WideFractionalPowerPreservesBinaryFraction(
        double power,
        double mantissa,
        long exponent
    )
    {
        var result = BigDouble.Pow(new BigDouble(1, 9007199254740993L), power);
        Assert.That(result.Mantissa, Is.EqualTo(mantissa).Within(3e-15));
        Assert.That(result.Exponent, Is.EqualTo(exponent));
    }
}
