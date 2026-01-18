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

        Assert.That(result.Exponent >= 93 && result.Exponent <= 95, Is.True, "1.000001e100 - 1e100 ≈ 1e94");
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
}
