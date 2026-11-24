using LD.Numeric.IdleNumber;

namespace LD.Numeric.Test;

/// <summary>
/// BigDouble 산술 연산 (Add, Subtract, Multiply, Divide) 테스트
/// </summary>
public class ArithmeticTests
{
    // ===== Add 테스트 =====

    [Test]
    public void Add_SameExponent()
    {
        BigDouble a = new BigDouble(1.5, 100);
        BigDouble b = new BigDouble(2.5, 100);
        BigDouble result = a + b;

        Assert.That(result == new BigDouble(4.0, 100), Is.True, "1.5e100 + 2.5e100 = 4e100");
    }

    [Test]
    public void Add_DifferentExponent_Small()
    {
        BigDouble a = new BigDouble(1.0, 100);
        BigDouble b = new BigDouble(1.0, 99);
        BigDouble result = a + b;

        Assert.That(result == new BigDouble(1.1, 100), Is.True, "1e100 + 1e99 = 1.1e100");
    }

    [Test]
    public void Add_DifferentExponent_Large()
    {
        BigDouble a = new BigDouble(1.0, 100);
        BigDouble b = new BigDouble(1.0, 80);
        BigDouble result = a + b;

        Assert.That(result == a, Is.True, "지수 차이 > 17이면 큰 값 반환");
    }

    // ===== Subtract 테스트 =====

    [Test]
    public void Subtract_SameValue_ShouldBeZero()
    {
        BigDouble a = new BigDouble("1.23456789e1000");
        BigDouble b = new BigDouble("1.23456789e1000");
        BigDouble result = a - b;

        Assert.That(result == BigDouble.Zero, Is.True, "같은 값 빼기 = 0");
    }

    [Test]
    public void Subtract_CloseValues()
    {
        BigDouble a = new BigDouble(1.000001, 100);
        BigDouble b = new BigDouble(1.0, 100);
        BigDouble result = a - b;

        Assert.That(
            result.Exponent >= 93 && result.Exponent <= 95,
            Is.True,
            "1.000001e100 - 1e100 ≈ 1e94"
        );
    }

    // ===== Multiply 테스트 =====

    [Test]
    public void Multiply_Basic()
    {
        BigDouble a = new BigDouble(2.0, 100);
        BigDouble b = new BigDouble(3.0, 50);
        BigDouble result = a * b;

        Assert.That(result == new BigDouble(6.0, 150), Is.True, "2e100 * 3e50 = 6e150");
    }

    [Test]
    public void Multiply_Overflow()
    {
        BigDouble a = new BigDouble(5.0, 100);
        BigDouble b = new BigDouble(5.0, 100);
        BigDouble result = a * b;

        Assert.That(result.Mantissa >= 2.4 && result.Mantissa <= 2.6, Is.True, "가수 2.5");
        Assert.That(result.Exponent == 201, Is.True, "지수 201");
    }

    // ===== Divide 테스트 =====

    [Test]
    public void Divide_Basic()
    {
        BigDouble a = new BigDouble(6.0, 150);
        BigDouble b = new BigDouble(2.0, 100);
        BigDouble result = a / b;

        Assert.That(result == new BigDouble(3.0, 50), Is.True, "6e150 / 2e100 = 3e50");
    }

    [Test]
    public void Divide_ByZero()
    {
        BigDouble a = new BigDouble(1.0, 100);
        BigDouble result = a / BigDouble.Zero;

        Assert.That(BigDouble.IsPositiveInfinity(result), Is.True, "양수 / 0 = +Inf");

        BigDouble negResult = (-a) / BigDouble.Zero;
        Assert.That(BigDouble.IsNegativeInfinity(negResult), Is.True, "음수 / 0 = -Inf");
    }

    [Test]
    public void ExponentLimits_NormalizationSaturates()
    {
        Assert.That(BigDouble.IsNegativeInfinity(new BigDouble(-10, long.MaxValue)), Is.True);
        Assert.That(new BigDouble(0.25, long.MinValue), Is.EqualTo(BigDouble.Zero));
        Assert.That(BigDouble.IsPositiveInfinity(new BigDouble(1000, long.MaxValue - 2)), Is.True);
    }

    [Test]
    public void ExponentLimits_AdditionPreservesMagnitude()
    {
        var largest = BigDouble.Pow10(long.MaxValue);
        var smallest = BigDouble.Pow10(long.MinValue);
        Assert.That(largest + smallest, Is.EqualTo(largest));
        Assert.That(smallest + smallest, Is.EqualTo(new BigDouble(2, long.MinValue)));
        Assert.That(smallest - smallest, Is.EqualTo(BigDouble.Zero));
        Assert.That(
            BigDouble.IsPositiveInfinity(new BigDouble(9, long.MaxValue) + largest),
            Is.True
        );
    }

    [Test]
    public void ExponentLimits_MultiplyAndDivideIncludeNormalizationCarry()
    {
        Assert.That(
            new BigDouble(5, long.MinValue) * new BigDouble(2, -1),
            Is.EqualTo(BigDouble.Pow10(long.MinValue))
        );
        Assert.That(
            new BigDouble(-1, long.MaxValue) / new BigDouble(2, -1),
            Is.EqualTo(new BigDouble(-5, long.MaxValue))
        );
        Assert.That(BigDouble.IsPositiveInfinity(new BigDouble(5, long.MaxValue) * 2), Is.True);
        Assert.That(BigDouble.Pow10(long.MinValue) / 2, Is.EqualTo(BigDouble.Zero));
    }

    [Test]
    public void ExponentLimits_ReciprocalHandlesMinimumExponent()
    {
        Assert.That(
            BigDouble.IsPositiveInfinity(BigDouble.Reciprocate(BigDouble.Pow10(long.MinValue))),
            Is.True
        );
        Assert.That(
            BigDouble.Reciprocate(new BigDouble(-2, long.MinValue)),
            Is.EqualTo(new BigDouble(-5, long.MaxValue))
        );
    }

    [Test]
    public void ExponentLimits_RootsRetainAllExponentDigits()
    {
        Assert.That(
            BigDouble.Sqrt(BigDouble.Pow10(long.MaxValue)).Exponent,
            Is.EqualTo(4611686018427387903L)
        );
        Assert.That(
            BigDouble.Cbrt(BigDouble.Pow10(long.MinValue)).Exponent,
            Is.EqualTo(-3074457345618258603L)
        );
        Assert.That(
            BigDouble.Cbrt(BigDouble.Pow10(long.MaxValue)).Exponent,
            Is.EqualTo(3074457345618258602L)
        );
    }
}
