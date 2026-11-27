using LD.Numeric.IdleNumber;

namespace LD.Numeric.Test;

public class MathFunctionTests
{
    [TestCase(1)]
    [TestCase(-1)]
    public void HyperbolicRange_TinyInputsKeepTheirMagnitude(int sign)
    {
        var value = new BigDouble(sign, -500);
        Assert.That(BigDouble.Sinh(value), Is.EqualTo(value));
        Assert.That(BigDouble.Tanh(value), Is.EqualTo(value));
        double small = sign * 1e-100;
        Assert.That(BigDouble.Asinh(small), Is.EqualTo(small));
        Assert.That(BigDouble.Atanh(small), Is.EqualTo(small));
    }

    [TestCase(1)]
    [TestCase(-1)]
    public void HyperbolicRange_LargeFiniteInputs(int sign)
    {
        var value = new BigDouble(sign, 400);
        Assert.That(BigDouble.Tanh(value), Is.EqualTo(new BigDouble(sign)));
        Assert.That(
            BigDouble.Asinh(value),
            Is.EqualTo(sign * (400 * Math.Log(10) + Math.Log(2))).Within(1e-12)
        );
        Assert.That(
            BigDouble.Acosh(BigDouble.Abs(value)),
            Is.EqualTo(400 * Math.Log(10) + Math.Log(2)).Within(1e-12)
        );
        Assert.That(
            BigDouble.Sinh(value),
            Is.EqualTo(new BigDouble(sign * double.PositiveInfinity))
        );
        Assert.That(BigDouble.Cosh(value), Is.EqualTo(BigDouble.PositiveInfinity));
    }

    [Test]
    public void HyperbolicRange_ModerateNegativeAsinhAvoidsCancellation()
    {
        Assert.That(BigDouble.Asinh(-1e10), Is.EqualTo(Math.Asinh(-1e10)).Within(1e-13));
        Assert.That(BigDouble.Sinh(1000).Exponent, Is.EqualTo(433));
        Assert.That(BigDouble.Cosh(1000).Mantissa, Is.EqualTo(9.850355570085235).Within(1e-11));
    }

    [Test]
    public void Pow_IntegerPower()
    {
        BigDouble a = new BigDouble(2.0, 10);
        BigDouble result = BigDouble.Pow(a, 3);

        Assert.That(result.Mantissa == 8.0, Is.True);
        Assert.That(result.Exponent == 30, Is.True);
    }

    [Test]
    public void Pow_FractionalPower()
    {
        BigDouble a = new BigDouble(4.0, 100);
        BigDouble result = BigDouble.Pow(a, 0.5);

        Assert.That(Math.Abs(result.Mantissa - 2.0) < 0.01, Is.True, "가수 2.0");
        Assert.That(result.Exponent == 50, Is.True, "지수 50");
    }

    [Test]
    public void Pow10_Basic()
    {
        BigDouble result = BigDouble.Pow10(100);

        Assert.That(result.Mantissa == 1.0, Is.True);
        Assert.That(result.Exponent == 100, Is.True);
    }

    [Test]
    public void Log10_Basic()
    {
        BigDouble a = new BigDouble(5.0, 100);
        double result = BigDouble.Log10(a);

        Assert.That(Math.Abs(result - 100.699) < 0.01, Is.True);
    }

    [Test]
    public void Sqrt_Basic()
    {
        BigDouble a = new BigDouble(4.0, 100);
        BigDouble result = BigDouble.Sqrt(a);

        Assert.That(Math.Abs(result.Mantissa - 2.0) < 0.01, Is.True);
        Assert.That(result.Exponent == 50, Is.True);
    }

    [Test]
    public void Sqrt_OddExponent()
    {
        BigDouble a = new BigDouble(4.0, 101);
        BigDouble result = BigDouble.Sqrt(a);

        Assert.That(result.Exponent == 50, Is.True);
    }

    [Test]
    public void Round_SmallExponent()
    {
        BigDouble a = new BigDouble(1.5, 2);
        BigDouble result = BigDouble.Round(a);

        Assert.That(result == new BigDouble(150), Is.True);
    }

    [Test]
    public void Round_LargeExponent()
    {
        BigDouble a = new BigDouble(1.5, 100);
        BigDouble result = BigDouble.Round(a);

        Assert.That(result == a, Is.True, "큰 지수는 반올림 무시");
    }

    [Test]
    public void Floor_Basic()
    {
        BigDouble a = new BigDouble(1.9, 2);
        BigDouble result = BigDouble.Floor(a);

        Assert.That(result == new BigDouble(190), Is.True);
    }

    [Test]
    public void Ceiling_Basic()
    {
        BigDouble a = new BigDouble(1.1, 2);
        BigDouble result = BigDouble.Ceiling(a);

        Assert.That(result == new BigDouble(110), Is.True);
    }

    [Test]
    public void Truncate_NegativeNumber()
    {
        BigDouble a = new BigDouble(-1.9, 2);
        BigDouble result = BigDouble.Truncate(a);

        Assert.That(result == new BigDouble(-190), Is.True);
    }

    [Test]
    public void HyperbolicRange_InverseFunctionsAtDomainEdges()
    {
        double above = Math.BitIncrement(1.0);
        Assert.That(BigDouble.Acosh(new BigDouble(above)), Is.EqualTo(Math.Acosh(above)));
        var below = new BigDouble(Math.BitDecrement(10.0), -1);
        double gap = (10 - below.Mantissa) / 10;
        double expected = Math.Log((2 - gap) / gap) / 2;
        Assert.That(BigDouble.Atanh(below), Is.EqualTo(expected).Within(5e-15));
        Assert.That(BigDouble.Atanh(-below), Is.EqualTo(-expected).Within(5e-15));
        Assert.That(BigDouble.IsNaN(BigDouble.Cosh(BigDouble.NaN)), Is.True);
    }
}
