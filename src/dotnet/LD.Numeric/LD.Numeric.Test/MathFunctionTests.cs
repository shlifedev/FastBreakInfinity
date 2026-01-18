using LD.Numeric.IdleNumber;

namespace LD.Numeric.Test;

/// <summary>
/// BigDouble 수학 함수 (Pow, Log, Sqrt, Round, Floor, Ceiling) 테스트
/// </summary>
public class MathFunctionTests
{
    // ===== Pow 테스트 =====

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

    // ===== Log 테스트 =====

    [Test]
    public void Log10_Basic()
    {
        BigDouble a = new BigDouble(5.0, 100);
        double result = BigDouble.Log10(a);

        Assert.That(Math.Abs(result - 100.699) < 0.01, Is.True);
    }

    // ===== Sqrt 테스트 =====

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

    // ===== Round/Floor/Ceiling 테스트 =====

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
}
