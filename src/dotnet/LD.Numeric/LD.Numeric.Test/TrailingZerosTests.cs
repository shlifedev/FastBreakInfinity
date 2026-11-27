using LD.Numeric.IdleNumber;

namespace LD.Numeric.Test;

public class TrailingZerosTests
{
    [Test]
    public void TrailingZeros_BasicEquality()
    {
        BigDouble a = new BigDouble("1.3120000e100");
        BigDouble b = new BigDouble("1.312e100");

        Assert.That(a == b, Is.True, "1.3120000e100 == 1.312e100");
        Assert.That(a.Equals(b), Is.True);
    }

    [Test]
    public void TrailingZeros_MultipleZeros()
    {
        BigDouble a = new BigDouble("1.500000000e50");
        BigDouble b = new BigDouble("1.5e50");

        Assert.That(a == b, Is.True, "1.500000000e50 == 1.5e50");
    }

    [Test]
    public void TrailingZeros_IntegerPart()
    {
        BigDouble a = new BigDouble("5.000000e10");
        BigDouble b = new BigDouble("5e10");

        Assert.That(a == b, Is.True, "5.000000e10 == 5e10");
    }

    [Test]
    public void TrailingZeros_ManyDecimalPlaces()
    {
        BigDouble a = new BigDouble("1.1234567890000000e100");
        BigDouble b = new BigDouble("1.123456789e100");

        Assert.That(a == b, Is.True, "trailing zeros 많은 경우");
    }

    [Test]
    public void TrailingZeros_LessThan()
    {
        BigDouble a = new BigDouble("1.5000e100");
        BigDouble b = new BigDouble("1.5e100");
        BigDouble c = new BigDouble("1.6e100");

        Assert.That(a < c, Is.True, "1.5000e100 < 1.6e100");
        Assert.That(a < b, Is.False, "1.5000e100 < 1.5e100 should be false (equal)");
    }

    [Test]
    public void TrailingZeros_GreaterThan()
    {
        BigDouble a = new BigDouble("1.5000e100");
        BigDouble b = new BigDouble("1.5e100");
        BigDouble c = new BigDouble("1.4e100");

        Assert.That(a > c, Is.True, "1.5000e100 > 1.4e100");
        Assert.That(a > b, Is.False, "1.5000e100 > 1.5e100 should be false (equal)");
    }

    [Test]
    public void TrailingZeros_LessOrEqual()
    {
        BigDouble a = new BigDouble("1.5000e100");
        BigDouble b = new BigDouble("1.5e100");
        BigDouble c = new BigDouble("1.6e100");

        Assert.That(a <= b, Is.True, "1.5000e100 <= 1.5e100 (equal)");
        Assert.That(a <= c, Is.True, "1.5000e100 <= 1.6e100");
    }

    [Test]
    public void TrailingZeros_GreaterOrEqual()
    {
        BigDouble a = new BigDouble("1.5000e100");
        BigDouble b = new BigDouble("1.5e100");
        BigDouble c = new BigDouble("1.4e100");

        Assert.That(a >= b, Is.True, "1.5000e100 >= 1.5e100 (equal)");
        Assert.That(a >= c, Is.True, "1.5000e100 >= 1.4e100");
    }

    [Test]
    public void TrailingZeros_Add()
    {
        BigDouble a = new BigDouble("1.5000e100");
        BigDouble b = new BigDouble("1.5e100");

        BigDouble result1 = a + new BigDouble("1e100");
        BigDouble result2 = b + new BigDouble("1e100");

        Assert.That(result1 == result2, Is.True, "덧셈 결과가 같아야 함");
        Assert.That(result1 == new BigDouble("2.5e100"), Is.True);
    }

    [Test]
    public void TrailingZeros_Subtract()
    {
        BigDouble a = new BigDouble("2.5000e100");
        BigDouble b = new BigDouble("2.5e100");

        BigDouble result1 = a - new BigDouble("1e100");
        BigDouble result2 = b - new BigDouble("1e100");

        Assert.That(result1 == result2, Is.True, "뺄셈 결과가 같아야 함");
        Assert.That(result1 == new BigDouble("1.5e100"), Is.True);
    }

    [Test]
    public void TrailingZeros_Multiply()
    {
        BigDouble a = new BigDouble("2.0000e50");
        BigDouble b = new BigDouble("2e50");

        BigDouble result1 = a * new BigDouble("3e50");
        BigDouble result2 = b * new BigDouble("3e50");

        Assert.That(result1 == result2, Is.True, "곱셈 결과가 같아야 함");
        Assert.That(result1 == new BigDouble("6e100"), Is.True);
    }

    [Test]
    public void TrailingZeros_Divide()
    {
        BigDouble a = new BigDouble("6.0000e100");
        BigDouble b = new BigDouble("6e100");

        BigDouble result1 = a / new BigDouble("2e50");
        BigDouble result2 = b / new BigDouble("2e50");

        Assert.That(result1 == result2, Is.True, "나눗셈 결과가 같아야 함");
        Assert.That(result1 == new BigDouble("3e50"), Is.True);
    }

    [Test]
    public void TrailingZeros_BothOperandsHaveTrailingZeros()
    {
        BigDouble a = new BigDouble("1.50000e100");
        BigDouble b = new BigDouble("2.50000e100");

        BigDouble sum = a + b;
        BigDouble expected = new BigDouble("4e100");

        Assert.That(sum == expected, Is.True, "1.5e100 + 2.5e100 = 4e100");
    }

    [Test]
    public void TrailingZeros_SubtractSameValue()
    {
        BigDouble a = new BigDouble("1.23456000e100");
        BigDouble b = new BigDouble("1.23456e100");

        BigDouble result = a - b;

        Assert.That(result == BigDouble.Zero, Is.True, "같은 값 빼기 = 0");
    }

    [Test]
    public void TrailingZeros_DifferentConstructors()
    {
        BigDouble a = new BigDouble("1.5000e10");

        BigDouble b = new BigDouble(1.5, 10);

        BigDouble c = new BigDouble(15000000000.0);

        Assert.That(a == b, Is.True, "문자열 vs (mantissa, exp) 생성자");
        Assert.That(b == c, Is.True, "(mantissa, exp) vs double 생성자");
        Assert.That(a == c, Is.True, "문자열 vs double 생성자");
    }

    [Test]
    public void TrailingZeros_NegativeNumbers()
    {
        BigDouble a = new BigDouble("-1.5000e100");
        BigDouble b = new BigDouble("-1.5e100");

        Assert.That(a == b, Is.True, "-1.5000e100 == -1.5e100");
        Assert.That(a < BigDouble.Zero, Is.True);
        Assert.That(b < BigDouble.Zero, Is.True);
    }

    [Test]
    public void TrailingZeros_VerySmallExponent()
    {
        BigDouble a = new BigDouble("1.5000e-100");
        BigDouble b = new BigDouble("1.5e-100");

        Assert.That(a == b, Is.True, "음수 지수에서도 동작");
    }

    [Test]
    public void TrailingZeros_ZeroValue()
    {
        BigDouble a = new BigDouble("0.0000e100");
        BigDouble b = new BigDouble("0e100");
        BigDouble c = BigDouble.Zero;

        Assert.That(a == b, Is.True, "0.0000e100 == 0e100");
        Assert.That(a == c, Is.True, "0.0000e100 == Zero");
    }

    [Test]
    public void TrailingZeros_OneValue()
    {
        BigDouble a = new BigDouble("1.0000e0");
        BigDouble b = new BigDouble("1e0");
        BigDouble c = BigDouble.One;

        Assert.That(a == b, Is.True, "1.0000e0 == 1e0");
        Assert.That(a == c, Is.True, "1.0000e0 == One");
    }

    [Test]
    public void TrailingZeros_Pow()
    {
        BigDouble a = new BigDouble("2.0000e10");
        BigDouble b = new BigDouble("2e10");

        BigDouble result1 = BigDouble.Pow(a, 2);
        BigDouble result2 = BigDouble.Pow(b, 2);

        Assert.That(result1 == result2, Is.True, "Pow 결과가 같아야 함");
    }

    [Test]
    public void TrailingZeros_Sqrt()
    {
        BigDouble a = new BigDouble("4.0000e100");
        BigDouble b = new BigDouble("4e100");

        BigDouble result1 = BigDouble.Sqrt(a);
        BigDouble result2 = BigDouble.Sqrt(b);

        Assert.That(result1 == result2, Is.True, "Sqrt 결과가 같아야 함");
    }

    [Test]
    public void TrailingZeros_GameScenario_PriceComparison()
    {
        BigDouble money = new BigDouble("100.000e50");
        BigDouble price = new BigDouble("100e50");

        Assert.That(money >= price, Is.True, "구매 가능해야 함");
        Assert.That(money == price, Is.True, "같은 값이어야 함");

        BigDouble remaining = money - price;
        Assert.That(remaining == BigDouble.Zero, Is.True, "잔액은 0이어야 함");
    }

    [Test]
    public void TrailingZeros_GameScenario_DamageCalculation()
    {
        BigDouble baseDamage = new BigDouble("1.5000e10");
        BigDouble multiplier = new BigDouble("2.0000e0");

        BigDouble totalDamage = baseDamage * multiplier;
        BigDouble expected = new BigDouble("3e10");

        Assert.That(totalDamage == expected, Is.True, "1.5e10 * 2 = 3e10");
    }
}
