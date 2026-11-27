using LD.Numeric.IdleNumber;

namespace LD.Numeric.Test;

public class ExponentStringPrecisionTests
{
    [Test]
    public void ExponentString_SameValue_DifferentNotation()
    {
        BigDouble a = new BigDouble("10e9");
        BigDouble b = new BigDouble("1e10");

        Assert.That(a == b, Is.True, "10e9 == 1e10");
        Assert.That(a.Equals(b), Is.True);
    }

    [Test]
    public void ExponentString_SameValue_MultipleNotations()
    {
        BigDouble a = new BigDouble("100e100");
        BigDouble b = new BigDouble("10e101");
        BigDouble c = new BigDouble("1e102");

        Assert.That(a == b, Is.True, "100e100 == 10e101");
        Assert.That(b == c, Is.True, "10e101 == 1e102");
        Assert.That(a == c, Is.True, "100e100 == 1e102");
    }

    [Test]
    public void ExponentString_FloatingPointNormalizationError()
    {
        BigDouble a = new BigDouble("11.23123123e9");
        BigDouble b = new BigDouble("1.123123123e10");

        double diff = Math.Abs(a.Mantissa - b.Mantissa);
        Assert.That(diff < 1e-15, Is.True, "정규화 오차는 1e-15 미만");
    }

    [Test]
    public void ExponentString_SlightlyDifferent_LastDigit()
    {
        BigDouble a = new BigDouble("1.3333192919230e100");
        BigDouble b = new BigDouble("1.3333192919231e100");

        Assert.That(a == b, Is.False, "마지막 자리 다름");
        Assert.That(a < b, Is.True);
    }

    [Test]
    public void ExponentString_SlightlyDifferent_DifferentExponent()
    {
        BigDouble a = new BigDouble("13.333192919230e99");
        BigDouble b = new BigDouble("1.3333192919231e100");

        Assert.That(a == b, Is.False, "다른 지수 표현도 미세 차이 감지");
        Assert.That(a < b, Is.True);
    }

    [Test]
    public void ExponentString_SlightlyDifferent_14thDigit()
    {
        BigDouble a = new BigDouble("1.0000000000000e100");
        BigDouble b = new BigDouble("1.0000000000001e100");

        Assert.That(a == b, Is.False, "14번째 자리 차이");
        Assert.That(a < b, Is.True);
    }

    [Test]
    public void ExponentString_SlightlyDifferent_15thDigit()
    {
        BigDouble a = new BigDouble("1.00000000000000e100");
        BigDouble b = new BigDouble("1.00000000000001e100");

        Assert.That(a.Mantissa, Is.Not.EqualTo(b.Mantissa));
        Assert.That(a == b, Is.False, "15번째 자리 차이");
        Assert.That(a < b, Is.True);
    }

    [Test]
    public void ExponentString_GreaterThan()
    {
        BigDouble a = new BigDouble("1.3333192919231e100");
        BigDouble b = new BigDouble("13.333192919230e99");

        Assert.That(a > b, Is.True, "1.3333192919231e100 > 1.3333192919230e100");
        Assert.That(b > a, Is.False);
    }

    [Test]
    public void ExponentString_LessThan()
    {
        BigDouble a = new BigDouble("13.333192919230e99");
        BigDouble b = new BigDouble("1.3333192919231e100");

        Assert.That(a < b, Is.True);
        Assert.That(b < a, Is.False);
    }

    [Test]
    public void ExponentString_GreaterOrEqual_Equal()
    {
        BigDouble a = new BigDouble("10e9");
        BigDouble b = new BigDouble("1e10");

        Assert.That(a >= b, Is.True, "같은 값이면 >= true");
        Assert.That(b >= a, Is.True);
    }

    [Test]
    public void ExponentString_LessOrEqual_Equal()
    {
        BigDouble a = new BigDouble("10e9");
        BigDouble b = new BigDouble("1e10");

        Assert.That(a <= b, Is.True, "같은 값이면 <= true");
        Assert.That(b <= a, Is.True);
    }

    [Test]
    public void ExponentString_UltraTiny_VariousExponents()
    {
        var testCases = new[]
        {
            ("1.234567890123456e10", "1.234567890123457e10"),
            ("1.234567890123456e100", "1.234567890123457e100"),
            ("1.234567890123456e1000", "1.234567890123457e1000"),
            ("1.234567890123456e-10", "1.234567890123457e-10"),
            ("1.234567890123456e-100", "1.234567890123457e-100"),
        };

        foreach (var (s1, s2) in testCases)
        {
            BigDouble a = new BigDouble(s1);
            BigDouble b = new BigDouble(s2);

            Assert.That(a.Mantissa, Is.Not.EqualTo(b.Mantissa), $"{s1} vs {s2}");
            Assert.That(a == b, Is.False, $"{s1} != {s2}");
            Assert.That(a < b, Is.True, $"{s1} < {s2}");
        }
    }

    [Test]
    public void ExponentString_UltraTiny_DifferentExponentNotation()
    {
        BigDouble a = new BigDouble("10.00000000000000e99");
        BigDouble b = new BigDouble("1.000000000000001e100");

        Assert.That(a == b, Is.False, "다른 지수 표기 극미세 차이");
        Assert.That(a < b, Is.True);
    }

    [Test]
    public void ExponentString_UltraTiny_Sequential()
    {
        var values = new[]
        {
            new BigDouble("1.0000000000000e100"),
            new BigDouble("1.0000000000001e100"),
            new BigDouble("1.0000000000002e100"),
            new BigDouble("1.0000000000003e100"),
            new BigDouble("1.0000000000004e100"),
        };

        for (int i = 0; i < values.Length - 1; i++)
        {
            Assert.That(values[i] < values[i + 1], Is.True, $"values[{i}] < values[{i + 1}]");
            Assert.That(values[i] == values[i + 1], Is.False);
        }
    }

    [Test]
    public void ExponentString_UltraTiny_NegativeExponent()
    {
        BigDouble a = new BigDouble("1.0000000000000e-100");
        BigDouble b = new BigDouble("1.0000000000001e-100");

        Assert.That(a == b, Is.False, "음수 지수 극미세 차이");
        Assert.That(a < b, Is.True);
    }

    [Test]
    public void ExponentString_UltraTiny_NegativeMantissa()
    {
        BigDouble a = new BigDouble("-1.0000000000001e100");
        BigDouble b = new BigDouble("-1.0000000000000e100");

        Assert.That(a == b, Is.False, "음수 mantissa 극미세 차이");
        Assert.That(a < b, Is.True, "-1.0000000000001 < -1.0");
    }

    [Test]
    public void ExponentString_MixedConstructors_Equal()
    {
        BigDouble a = new BigDouble("1.123456789e100");
        BigDouble b = new BigDouble(1.123456789, 100);

        Assert.That(a == b, Is.True, "문자열 vs 생성자 같은 값");
    }

    [Test]
    public void ExponentString_MixedConstructors_SlightlyDifferent()
    {
        BigDouble a = new BigDouble("1.123456789012345e100");
        BigDouble b = new BigDouble(1.123456789012346, 100);

        Assert.That(a.Mantissa, Is.Not.EqualTo(b.Mantissa));
        Assert.That(a == b, Is.False, "혼합 생성자 미세 차이");
        Assert.That(a < b, Is.True);
    }

    [Test]
    public void ExponentString_GameScenario_PriceComparison()
    {
        BigDouble money = new BigDouble("99.99999999999e98");
        BigDouble price = new BigDouble("1.0e100");

        Assert.That(money < price, Is.True, "돈 부족");
        Assert.That(money >= price, Is.False, "구매 불가");
    }

    [Test]
    public void ExponentString_GameScenario_DamageCalculation()
    {
        BigDouble baseDamage = new BigDouble("15.5e49");
        BigDouble multiplier = new BigDouble("20.0e-1");
        BigDouble expected = new BigDouble("3.1e50");

        BigDouble actual = baseDamage * multiplier;

        Assert.That(actual == expected, Is.True, "15.5e49 * 2 = 3.1e50");
    }

    [Test]
    public void ExponentString_GameScenario_CriticalHit()
    {
        BigDouble threshold = new BigDouble("1.0000000000000e100");
        BigDouble roll1 = new BigDouble("0.9999999999999e100");
        BigDouble roll2 = new BigDouble("1.0000000000001e100");

        Assert.That(roll1 < threshold, Is.True, "roll1은 threshold 미만");
        Assert.That(roll2 > threshold, Is.True, "roll2는 threshold 초과");
        Assert.That(roll1 == threshold, Is.False);
        Assert.That(roll2 == threshold, Is.False);
    }

    [Test]
    public void ExponentString_NormalizedComparison()
    {
        BigDouble a = new BigDouble("100e98");
        BigDouble b = new BigDouble("10e99");
        BigDouble c = new BigDouble("1e100");

        Assert.That(a == b, Is.True, "100e98 == 10e99");
        Assert.That(b == c, Is.True, "10e99 == 1e100");
        Assert.That(a == c, Is.True, "100e98 == 1e100");
    }

    [Test]
    public void ExponentString_NormalizedComparison_WithDecimal()
    {
        BigDouble c = new BigDouble("1e100");
        BigDouble d = new BigDouble("0.1e101");

        double diff = Math.Abs(c.Mantissa - d.Mantissa);

        Assert.That(diff < 1e-14, Is.True, "정규화 오차 확인");
    }

    [Test]
    public void ExponentString_NormalizedComparison_SlightlyDifferent()
    {
        BigDouble a = new BigDouble("100.0000000001e98");
        BigDouble b = new BigDouble("1e100");

        Assert.That(a == b, Is.False, "정규화 후 미세 차이");
        Assert.That(a > b, Is.True);
    }

    [Test]
    public void ExponentString_CompareTo_Consistency()
    {
        BigDouble a = new BigDouble("13.333192919231e99");
        BigDouble b = new BigDouble("1.3333192919230e100");

        int compare = a.CompareTo(b);

        Assert.That(compare > 0, Is.True, "a > b이면 CompareTo > 0");
        Assert.That(a > b, Is.True);
        Assert.That(a >= b, Is.True);
        Assert.That(a < b, Is.False);
        Assert.That(a <= b, Is.False);
        Assert.That(a == b, Is.False);
        Assert.That(a != b, Is.True);
    }

    [Test]
    public void ExponentString_ExtremeExponent_Large()
    {
        BigDouble a = new BigDouble("1.0000000000000e999999999");
        BigDouble b = new BigDouble("1.0000000000001e999999999");

        Assert.That(a == b, Is.False, "큰 지수에서 미세 차이");
        Assert.That(a < b, Is.True);
    }

    [Test]
    public void ExponentString_ExtremeExponent_Small()
    {
        BigDouble a = new BigDouble("1.0000000000000e-999999999");
        BigDouble b = new BigDouble("1.0000000000001e-999999999");

        Assert.That(a == b, Is.False, "작은 지수에서 미세 차이");
        Assert.That(a < b, Is.True);
    }

    [Test]
    public void ExponentString_ExtremeExponent_SameValue()
    {
        BigDouble a = new BigDouble("10e999999998");
        BigDouble b = new BigDouble("1e999999999");

        Assert.That(a == b, Is.True, "극단적 지수에서 같은 값");
    }
}
