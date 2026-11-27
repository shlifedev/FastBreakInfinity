using LD.Numeric.IdleNumber;

namespace LD.Numeric.Test;

public class PrecisionTests
{
    private static double NextUp(double value)
    {
        return BitConverter.Int64BitsToDouble(BitConverter.DoubleToInt64Bits(value) + 1);
    }

    [Test]
    public void Precision_SlightlyDifferentMantissa_NotEqual()
    {
        BigDouble a = new BigDouble(1.3333192919231, 100);
        BigDouble b = new BigDouble(1.3333192919230, 100);

        Assert.That(a == b, Is.False, "미세하게 다른 값은 같지 않아야 함");
        Assert.That(a != b, Is.True);
        Assert.That(a.Equals(b), Is.False);
    }

    [Test]
    public void Precision_LastDigitDifferent_NotEqual()
    {
        BigDouble a = new BigDouble(1.123456789012341, 50);
        BigDouble b = new BigDouble(1.123456789012340, 50);

        Assert.That(a == b, Is.False, "마지막 자리 다르면 같지 않아야 함");
        Assert.That(a != b, Is.True);
    }

    [Test]
    public void Precision_VerySmallDifference_NotEqual()
    {
        BigDouble a = new BigDouble(1.0000000000001, 100);
        BigDouble b = new BigDouble(1.0000000000000, 100);

        Assert.That(a == b, Is.False, "1e-13 차이도 감지해야 함");
    }

    [Test]
    public void Precision_StringParsing_SlightlyDifferent()
    {
        BigDouble a = new BigDouble("1.3333192919231e100");
        BigDouble b = new BigDouble("1.3333192919230e100");

        Assert.That(a == b, Is.False, "문자열 파싱도 미세한 차이 감지해야 함");
        Assert.That(a != b, Is.True);
    }

    [Test]
    public void Precision_GreaterThan_SlightlyLarger()
    {
        BigDouble a = new BigDouble(1.3333192919231, 100);
        BigDouble b = new BigDouble(1.3333192919230, 100);

        Assert.That(a > b, Is.True, "1.3333192919231 > 1.3333192919230");
        Assert.That(b > a, Is.False);
    }

    [Test]
    public void Precision_LessThan_SlightlySmaller()
    {
        BigDouble a = new BigDouble(1.3333192919230, 100);
        BigDouble b = new BigDouble(1.3333192919231, 100);

        Assert.That(a < b, Is.True, "1.3333192919230 < 1.3333192919231");
        Assert.That(b < a, Is.False);
    }

    [Test]
    public void Precision_GreaterOrEqual_SlightlyDifferent()
    {
        BigDouble a = new BigDouble(1.3333192919231, 100);
        BigDouble b = new BigDouble(1.3333192919230, 100);

        Assert.That(a >= b, Is.True, "더 큰 값은 >= true");
        Assert.That(b >= a, Is.False, "더 작은 값은 >= false");
    }

    [Test]
    public void Precision_LessOrEqual_SlightlyDifferent()
    {
        BigDouble a = new BigDouble(1.3333192919230, 100);
        BigDouble b = new BigDouble(1.3333192919231, 100);

        Assert.That(a <= b, Is.True, "더 작은 값은 <= true");
        Assert.That(b <= a, Is.False, "더 큰 값은 <= false");
    }

    [Test]
    public void Precision_AtToleranceBoundary()
    {
        BigDouble a = new BigDouble(1.0, 100);
        BigDouble b = new BigDouble(1.0 + 1e-15, 100);

        Assert.That(a == b, Is.False, "1e-15 차이는 다르게 판정");
    }

    [Test]
    public void Precision_BelowTolerance_Equal()
    {
        BigDouble a = new BigDouble(1.0, 100);
        double roundedMantissa = 1.0 + 1e-19;
        BigDouble b = new BigDouble(1.0 + 1e-19, 100);

        Assert.That(roundedMantissa, Is.EqualTo(1.0), "1e-19 차이는 double로 표현되지 않음");
        Assert.That(a.Mantissa, Is.EqualTo(b.Mantissa));
        Assert.That(a == b, Is.True);
    }

    [Test]
    public void Precision_GameScenario_DamageComparison()
    {
        BigDouble damage1 = new BigDouble("1.999999999999e50");
        BigDouble damage2 = new BigDouble("2.000000000000e50");

        Assert.That(damage1 < damage2, Is.True, "1.999... < 2.0");
        Assert.That(damage1 == damage2, Is.False, "미세한 차이는 다름");
    }

    [Test]
    public void Precision_GameScenario_MoneyCheck()
    {
        BigDouble money = new BigDouble("9.999999999999e100");
        BigDouble price = new BigDouble("1e101");

        Assert.That(money < price, Is.True, "돈 부족");
        Assert.That(money >= price, Is.False, "구매 불가");
    }

    [Test]
    public void Precision_GameScenario_ProgressCheck()
    {
        BigDouble current = new BigDouble(9.99999, 10);
        BigDouble target = new BigDouble(1.0, 11);

        Assert.That(current < target, Is.True, "목표 미달성");
        Assert.That(current == target, Is.False);
    }

    [Test]
    public void Precision_ChainedValues_StrictOrdering()
    {
        BigDouble v1 = new BigDouble(1.0000000000000, 100);
        BigDouble v2 = new BigDouble(1.0000000000001, 100);
        BigDouble v3 = new BigDouble(1.0000000000002, 100);
        BigDouble v4 = new BigDouble(1.0000000000003, 100);

        Assert.That(v1 < v2, Is.True);
        Assert.That(v2 < v3, Is.True);
        Assert.That(v3 < v4, Is.True);
        Assert.That(v1 < v4, Is.True);

        Assert.That(v4 > v3, Is.True);
        Assert.That(v3 > v2, Is.True);
        Assert.That(v2 > v1, Is.True);
    }

    [Test]
    public void Precision_CompareTo_Consistency()
    {
        BigDouble a = new BigDouble(1.3333192919231, 100);
        BigDouble b = new BigDouble(1.3333192919230, 100);

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
    public void Precision_UltraTiny_16thDigit()
    {
        BigDouble a = new BigDouble(1.0000000000000000, 100);
        BigDouble b = new BigDouble(1.0000000000000002, 100);

        Assert.That(a.Mantissa, Is.Not.EqualTo(b.Mantissa));
        Assert.That(a == b, Is.False, "16번째 자리 차이 감지");
        Assert.That(a < b, Is.True);
    }

    [Test]
    public void Precision_UltraTiny_15thDigit()
    {
        BigDouble a = new BigDouble(1.00000000000000, 100);
        BigDouble b = new BigDouble(1.00000000000001, 100);

        Assert.That(a == b, Is.False, "15번째 자리 차이 감지");
        Assert.That(a < b, Is.True);
    }

    [Test]
    public void Precision_UltraTiny_14thDigit()
    {
        BigDouble a = new BigDouble(1.0000000000000, 100);
        BigDouble b = new BigDouble(1.0000000000001, 100);

        Assert.That(a == b, Is.False, "14번째 자리 차이 감지");
        Assert.That(a < b, Is.True);
        Assert.That(b > a, Is.True);
    }

    [Test]
    public void Precision_UltraTiny_13thDigit()
    {
        BigDouble a = new BigDouble(1.000000000000, 100);
        BigDouble b = new BigDouble(1.000000000001, 100);

        Assert.That(a == b, Is.False, "13번째 자리 차이 감지");
        Assert.That(a < b, Is.True);
    }

    [Test]
    public void Precision_UltraTiny_VariousMantissa()
    {
        var testCases = new[]
        {
            (1.234567890123456, NextUp(1.234567890123456)),
            (9.999999999999998, NextUp(9.999999999999998)),
            (1.111111111111111, NextUp(1.111111111111111)),
            (5.555555555555555, NextUp(5.555555555555555)),
        };

        foreach (var (m1, m2) in testCases)
        {
            BigDouble a = new BigDouble(m1, 100);
            BigDouble b = new BigDouble(m2, 100);

            Assert.That(a.Mantissa, Is.Not.EqualTo(b.Mantissa), $"{m1:R} vs {m2:R}");
            Assert.That(a == b, Is.False, $"{m1} != {m2}");
            Assert.That(a < b, Is.True, $"{m1} < {m2}");
        }
    }

    [Test]
    public void Precision_UltraTiny_NearOne()
    {
        double one = 1.0;
        double nextUp = NextUp(one);

        BigDouble a = new BigDouble(one, 100);
        BigDouble b = new BigDouble(nextUp, 100);

        Assert.That(a == b, Is.False, "1비트 차이도 감지");
        Assert.That(a < b, Is.True);
    }

    [Test]
    public void Precision_UltraTiny_StringParsing()
    {
        BigDouble a = new BigDouble("1.234567890123456e100");
        BigDouble b = new BigDouble("1.234567890123457e100");

        Assert.That(a.Mantissa, Is.Not.EqualTo(b.Mantissa));
        Assert.That(a == b, Is.False, "문자열 파싱 극미세 차이");
        Assert.That(a < b, Is.True);
    }

    [Test]
    public void Precision_UltraTiny_Sequential()
    {
        double baseVal = 1.0;
        var values = new List<BigDouble>();

        for (int i = 0; i < 10; i++)
        {
            double val = BitConverter.Int64BitsToDouble(
                BitConverter.DoubleToInt64Bits(baseVal) + i
            );
            values.Add(new BigDouble(val, 100));
        }

        for (int i = 0; i < values.Count - 1; i++)
        {
            Assert.That(values[i] < values[i + 1], Is.True, $"values[{i}] < values[{i + 1}]");
            Assert.That(values[i] == values[i + 1], Is.False);
            Assert.That(values[i] != values[i + 1], Is.True);
        }
    }

    [Test]
    public void Precision_UltraTiny_LargeExponent()
    {
        BigDouble a = new BigDouble(1.000000000000000, 999999);
        BigDouble b = new BigDouble(1.000000000000001, 999999);

        Assert.That(a == b, Is.False, "큰 지수에서도 극미세 차이 감지");
        Assert.That(a < b, Is.True);
    }

    [Test]
    public void Precision_UltraTiny_SmallExponent()
    {
        BigDouble a = new BigDouble(1.000000000000000, -999999);
        BigDouble b = new BigDouble(1.000000000000001, -999999);

        Assert.That(a == b, Is.False, "작은 지수에서도 극미세 차이 감지");
        Assert.That(a < b, Is.True);
    }

    [Test]
    public void Precision_UltraTiny_NegativeValues()
    {
        BigDouble a = new BigDouble(-1.000000000000001, 100);
        BigDouble b = new BigDouble(-1.000000000000000, 100);

        Assert.That(a == b, Is.False, "음수에서 극미세 차이");
        Assert.That(a < b, Is.True, "-1.000000000000001 < -1.0");
        Assert.That(b > a, Is.True);
    }

    [Test]
    public void Precision_UltraTiny_Arithmetic()
    {
        BigDouble a = new BigDouble(1.00000000000, 100);
        BigDouble b = new BigDouble(1.00000000001, 100);

        BigDouble sum_a = a + new BigDouble(1, 100);
        BigDouble sum_b = b + new BigDouble(1, 100);

        Assert.That(sum_a == sum_b, Is.False, "덧셈 후 차이 유지");
        Assert.That(sum_a < sum_b, Is.True);
    }

    [Test]
    public void Precision_UltraTiny_GameScenario_CriticalDamage()
    {
        BigDouble bossHP = new BigDouble(1.000000000000000, 1000);
        BigDouble damage1 = new BigDouble(0.999999999999999, 1000);
        BigDouble damage2 = new BigDouble(1.000000000000000, 1000);

        Assert.That(damage1 < bossHP, Is.True, "데미지1은 보스HP 미달");
        Assert.That(damage2 >= bossHP, Is.True, "데미지2는 보스HP 이상");
        Assert.That(damage1 == damage2, Is.False, "두 데미지는 다름");
    }
}
