using LD.Numeric.IdleNumber;

namespace LD.Numeric.Test;

public class BugHuntRegressionTests
{
    [Test]
    public void Cbrt_NegativeExponent()
    {
        Assert.That(
            BigDouble.Cbrt(new BigDouble(1e-4)).ToDouble(),
            Is.EqualTo(Math.Cbrt(1e-4)).Within(1e-16)
        );
        Assert.That(
            BigDouble.Cbrt(new BigDouble(-1e-2)).ToDouble(),
            Is.EqualTo(Math.Cbrt(-1e-2)).Within(1e-16)
        );
    }

    [Test]
    public void Cbrt_PowerOfTenSweep()
    {
        for (int e = -30; e <= 30; e++)
        {
            double expected = Math.Cbrt(Math.Pow(10, e));
            Assert.That(
                BigDouble.Cbrt(BigDouble.Pow10((long)e)).ToDouble(),
                Is.EqualTo(expected).Within(Math.Abs(expected) * 1e-12),
                $"Cbrt(1e{e})"
            );
        }
    }

    [Test]
    public void Normalize_Subnormal_KeepsMantissaInvariant()
    {
        var a = new BigDouble(1e-323);
        Assert.That(a.Mantissa, Is.GreaterThanOrEqualTo(1).And.LessThan(10));
        Assert.That(a.ToDouble(), Is.EqualTo(1e-323));

        var b = new BigDouble(1e-322);
        Assert.That(b.Mantissa, Is.GreaterThanOrEqualTo(1).And.LessThan(10));
        Assert.That(b.ToDouble(), Is.EqualTo(1e-322));

        Assert.That(new BigDouble(5e-324).ToDouble(), Is.EqualTo(5e-324));
    }

    [Test]
    public void Multiply_NaN_Propagates()
    {
        Assert.That(BigDouble.IsNaN(BigDouble.NaN * new BigDouble(0.5)), Is.True);
        Assert.That(BigDouble.IsNaN(new BigDouble(0.5) * BigDouble.NaN), Is.True);
    }

    [Test]
    public void Pow_NaN_Propagates()
    {
        Assert.That(BigDouble.IsNaN(BigDouble.Pow(BigDouble.NaN, 2L)), Is.True);
    }

    [Test]
    public void Pow_NegativeBase_LargeOddPower_KeepsSign()
    {
        var result = BigDouble.Pow(new BigDouble(-9), 401L);
        Assert.That(result.Mantissa, Is.LessThan(0));
        Assert.That(result.Exponent, Is.EqualTo(382));
        Assert.That(result.Mantissa, Is.EqualTo(-4.4796727106).Within(1e-4));

        Assert.That(BigDouble.Pow(new BigDouble(-9), 400L).Mantissa, Is.GreaterThan(0));
    }

    [Test]
    public void Log_BaseOne_IsNaN()
    {
        Assert.That(double.IsNaN(BigDouble.Log(new BigDouble(100), 1.0)), Is.True);
    }

    [Test]
    public void GetHashCode_AllZerosShareHash()
    {
        var z1 = BigDouble.Zero;
        var z2 = BigDouble.FromMantissaExponentNoNormalize(0, 5);
        Assert.That(z1.Equals(z2), Is.True);
        Assert.That(z1.GetHashCode(), Is.EqualTo(z2.GetHashCode()));
    }

    [Test]
    public void ToString_MantissaRoundingCarriesUnit()
    {
        Assert.That(new BigDouble("9.999999e23").ToString(), Is.EqualTo("1.00H"));
    }

    [Test]
    public void ConvertToAlphabetUnit_RoundingCarriesUnit()
    {
        Assert.That(999999.0.ConvertToAlphabetUnit(), Is.EqualTo("1.00B"));
        Assert.That(999994.0.ConvertToAlphabetUnit(), Is.EqualTo("999.99A"));
    }

    [Test]
    public void ConvertToAlphabetUnit_NegativeValues()
    {
        Assert.That((-5000.0).ConvertToAlphabetUnit(), Is.EqualTo("-5.00A"));
        Assert.That((-1500000.0).ConvertToAlphabetUnit(), Is.EqualTo("-1.50B"));
        Assert.That((-500.0).ConvertToAlphabetUnit(), Is.EqualTo("-500"));
    }

    [Test]
    public void ConvertFromAlphabetUnit_LowercaseUnit_Throws()
    {
        Assert.Throws<FormatException>(() => AlphabetConverter.ConvertFromAlphabetUnit("1.5a"));
    }

    [Test]
    public void AdjustedMantissa_NegativeExponent_ReturnsActualValue()
    {
        Assert.That(new BigDouble(0.5).AdjustedMantissa(), Is.EqualTo(0.5));
    }

    [Test]
    public void GetDigits_LargeNegative_NoCrash()
    {
        Assert.That(NumberUtility.GetDigits(-3e9), Is.EqualTo(10));
        Assert.That(NumberUtility.GetDigits(int.MinValue), Is.EqualTo(10));
    }

    [Test]
    public void GetDigits_BeyondIntRange()
    {
        Assert.That(NumberUtility.GetDigits(1e10), Is.EqualTo(11));
        Assert.That(NumberUtility.GetDigits(1e15), Is.EqualTo(16));
        Assert.That(NumberUtility.GetDigits(999.0), Is.EqualTo(3));
        Assert.That(NumberUtility.GetDigits(0.5), Is.EqualTo(0));
    }
}
