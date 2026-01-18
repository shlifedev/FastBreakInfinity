using LD.Numeric.IdleNumber;

namespace LD.Numeric.Test;

/// <summary>
/// 소수점 자릿수가 달라도 같은 값인 경우 테스트 (Trailing Zeros)
/// 예: 1.312300 == 1.312, 1.50000 == 1.5
/// </summary>
public class TrailingZerosTests
{
    // ===== 기본 동등성 테스트 =====

    [Test]
    public void TrailingZeros_BasicEquality()
    {
        // 기본 케이스: trailing zeros (실제 0이 뒤에 붙은 경우)
        // 주의: "1.312300"은 1.3123이므로 올바른 trailing zeros가 아님
        // 올바른 예: "1.3120000" == "1.312" (둘 다 1.312)
        BigDouble a = new BigDouble("1.3120000e100");
        BigDouble b = new BigDouble("1.312e100");

        Assert.That(a == b, Is.True, "1.3120000e100 == 1.312e100");
        Assert.That(a.Equals(b), Is.True);
    }

    [Test]
    public void TrailingZeros_MultipleZeros()
    {
        // 여러 개의 trailing zeros
        BigDouble a = new BigDouble("1.500000000e50");
        BigDouble b = new BigDouble("1.5e50");

        Assert.That(a == b, Is.True, "1.500000000e50 == 1.5e50");
    }

    [Test]
    public void TrailingZeros_IntegerPart()
    {
        // 정수부만 있는 경우
        BigDouble a = new BigDouble("5.000000e10");
        BigDouble b = new BigDouble("5e10");

        Assert.That(a == b, Is.True, "5.000000e10 == 5e10");
    }

    [Test]
    public void TrailingZeros_ManyDecimalPlaces()
    {
        // 많은 소수점 자릿수 끝에 trailing zeros
        BigDouble a = new BigDouble("1.1234567890000000e100");
        BigDouble b = new BigDouble("1.123456789e100");

        Assert.That(a == b, Is.True, "trailing zeros 많은 경우");
    }

    // ===== 비교 연산자 테스트 =====

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

    // ===== 산술 연산 테스트 =====

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

    // ===== 혼합 케이스 =====

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

    // ===== 생성자 방식 비교 =====

    [Test]
    public void TrailingZeros_DifferentConstructors()
    {
        // 문자열 생성자
        BigDouble a = new BigDouble("1.5000e10");

        // mantissa, exponent 생성자
        BigDouble b = new BigDouble(1.5, 10);

        // double 생성자로 15000000000 만들기
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

    // ===== 경계 케이스 =====

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

    // ===== Pow/Sqrt 연산 =====

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

    // ===== 실제 사용 케이스 =====

    [Test]
    public void TrailingZeros_GameScenario_PriceComparison()
    {
        // 게임에서 가격 비교 시나리오
        BigDouble money = new BigDouble("100.000e50");
        BigDouble price = new BigDouble("100e50");

        Assert.That(money >= price, Is.True, "구매 가능해야 함");
        Assert.That(money == price, Is.True, "같은 값이어야 함");

        // 구매 후 잔액
        BigDouble remaining = money - price;
        Assert.That(remaining == BigDouble.Zero, Is.True, "잔액은 0이어야 함");
    }

    [Test]
    public void TrailingZeros_GameScenario_DamageCalculation()
    {
        // 데미지 계산 시나리오
        BigDouble baseDamage = new BigDouble("1.5000e10");
        BigDouble multiplier = new BigDouble("2.0000e0");

        BigDouble totalDamage = baseDamage * multiplier;
        BigDouble expected = new BigDouble("3e10");

        Assert.That(totalDamage == expected, Is.True, "1.5e10 * 2 = 3e10");
    }
}
