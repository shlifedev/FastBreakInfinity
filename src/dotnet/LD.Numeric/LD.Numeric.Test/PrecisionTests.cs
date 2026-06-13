using LD.Numeric.IdleNumber;

namespace LD.Numeric.Test;

/// <summary>
/// 정밀도 비교 테스트
/// 미세하게 다른 값이 같다고 판정되거나, 크지 않은데 크다고 판정되는 경우가 없어야 함
/// </summary>
public class PrecisionTests
{
    private static double NextUp(double value)
    {
        return BitConverter.Int64BitsToDouble(BitConverter.DoubleToInt64Bits(value) + 1);
    }

    // ===== 미세한 차이 동등성 테스트 =====

    [Test]
    public void Precision_SlightlyDifferentMantissa_NotEqual()
    {
        // 1.3333192919231 != 1.3333192919230
        BigDouble a = new BigDouble(1.3333192919231, 100);
        BigDouble b = new BigDouble(1.3333192919230, 100);

        Assert.That(a == b, Is.False, "미세하게 다른 값은 같지 않아야 함");
        Assert.That(a != b, Is.True);
        Assert.That(a.Equals(b), Is.False);
    }

    [Test]
    public void Precision_LastDigitDifferent_NotEqual()
    {
        // 마지막 자리만 다른 경우
        BigDouble a = new BigDouble(1.123456789012341, 50);
        BigDouble b = new BigDouble(1.123456789012340, 50);

        Assert.That(a == b, Is.False, "마지막 자리 다르면 같지 않아야 함");
        Assert.That(a != b, Is.True);
    }

    [Test]
    public void Precision_VerySmallDifference_NotEqual()
    {
        // 아주 작은 차이도 감지해야 함
        BigDouble a = new BigDouble(1.0000000000001, 100);
        BigDouble b = new BigDouble(1.0000000000000, 100);

        Assert.That(a == b, Is.False, "1e-13 차이도 감지해야 함");
    }

    [Test]
    public void Precision_StringParsing_SlightlyDifferent()
    {
        // 문자열 파싱에서 미세한 차이
        BigDouble a = new BigDouble("1.3333192919231e100");
        BigDouble b = new BigDouble("1.3333192919230e100");

        Assert.That(a == b, Is.False, "문자열 파싱도 미세한 차이 감지해야 함");
        Assert.That(a != b, Is.True);
    }

    // ===== 비교 연산자 정밀도 테스트 =====

    [Test]
    public void Precision_GreaterThan_SlightlyLarger()
    {
        // 미세하게 큰 값이 크다고 판정되어야 함
        BigDouble a = new BigDouble(1.3333192919231, 100);
        BigDouble b = new BigDouble(1.3333192919230, 100);

        Assert.That(a > b, Is.True, "1.3333192919231 > 1.3333192919230");
        Assert.That(b > a, Is.False);
    }

    [Test]
    public void Precision_LessThan_SlightlySmaller()
    {
        // 미세하게 작은 값이 작다고 판정되어야 함
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

    // ===== 경계 케이스 =====

    [Test]
    public void Precision_AtToleranceBoundary()
    {
        // double 정밀도 한계: 약 15-16 유효숫자
        // 1.0 근처에서 표현 가능한 최소 차이는 약 2.2e-16
        // 1e-17은 double로 표현 불가능 (1.0 + 1e-17 == 1.0)
        BigDouble a = new BigDouble(1.0, 100);
        BigDouble b = new BigDouble(1.0 + 1e-15, 100); // 1e-15 차이 (표현 가능)

        // 1e-15 > 1e-18 (Tolerance)이므로 다르게 판정되어야 함
        Assert.That(a == b, Is.False, "1e-15 차이는 다르게 판정");
    }

    [Test]
    public void Precision_BelowTolerance_Equal()
    {
        // Tolerance(1e-18) 미만 차이는 같다고 판정될 수 있음
        BigDouble a = new BigDouble(1.0, 100);
        double roundedMantissa = 1.0 + 1e-19;
        BigDouble b = new BigDouble(1.0 + 1e-19, 100); // 1e-19 차이

        Assert.That(roundedMantissa, Is.EqualTo(1.0), "1e-19 차이는 double로 표현되지 않음");
        Assert.That(a.Mantissa, Is.EqualTo(b.Mantissa));
        Assert.That(a == b, Is.True);
    }

    // ===== 실제 게임 시나리오 =====

    [Test]
    public void Precision_GameScenario_DamageComparison()
    {
        // 데미지 비교: 미세한 차이로 승패가 갈릴 수 있음
        BigDouble damage1 = new BigDouble("1.999999999999e50");
        BigDouble damage2 = new BigDouble("2.000000000000e50");

        Assert.That(damage1 < damage2, Is.True, "1.999... < 2.0");
        Assert.That(damage1 == damage2, Is.False, "미세한 차이는 다름");
    }

    [Test]
    public void Precision_GameScenario_MoneyCheck()
    {
        // 돈 부족 체크: 0.000001 부족해도 구매 불가
        BigDouble money = new BigDouble("9.999999999999e100");
        BigDouble price = new BigDouble("1e101"); // 10e100

        Assert.That(money < price, Is.True, "돈 부족");
        Assert.That(money >= price, Is.False, "구매 불가");
    }

    [Test]
    public void Precision_GameScenario_ProgressCheck()
    {
        // 진행도 체크: 99.9999%와 100%는 다름
        BigDouble current = new BigDouble(9.99999, 10); // 99.9999e9
        BigDouble target = new BigDouble(1.0, 11); // 100e9 = 1e11

        Assert.That(current < target, Is.True, "목표 미달성");
        Assert.That(current == target, Is.False);
    }

    // ===== 연속 비교 테스트 =====

    [Test]
    public void Precision_ChainedValues_StrictOrdering()
    {
        // 연속적인 값들이 올바른 순서를 유지하는지
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
        // CompareTo와 비교 연산자 일관성
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

    // ===== 극미세 차이 테스트 (double 정밀도 한계 근처) =====

    [Test]
    public void Precision_UltraTiny_16thDigit()
    {
        // 16번째 유효숫자 차이 (double 정밀도 한계)
        BigDouble a = new BigDouble(1.0000000000000000, 100);
        BigDouble b = new BigDouble(1.0000000000000002, 100); // 마지막 자리 2

        Assert.That(a.Mantissa, Is.Not.EqualTo(b.Mantissa));
        Assert.That(a == b, Is.False, "16번째 자리 차이 감지");
        Assert.That(a < b, Is.True);
    }

    [Test]
    public void Precision_UltraTiny_15thDigit()
    {
        // 15번째 유효숫자 차이
        BigDouble a = new BigDouble(1.00000000000000, 100);
        BigDouble b = new BigDouble(1.00000000000001, 100);

        Assert.That(a == b, Is.False, "15번째 자리 차이 감지");
        Assert.That(a < b, Is.True);
    }

    [Test]
    public void Precision_UltraTiny_14thDigit()
    {
        // 14번째 유효숫자 차이
        BigDouble a = new BigDouble(1.0000000000000, 100);
        BigDouble b = new BigDouble(1.0000000000001, 100);

        Assert.That(a == b, Is.False, "14번째 자리 차이 감지");
        Assert.That(a < b, Is.True);
        Assert.That(b > a, Is.True);
    }

    [Test]
    public void Precision_UltraTiny_13thDigit()
    {
        // 13번째 유효숫자 차이
        BigDouble a = new BigDouble(1.000000000000, 100);
        BigDouble b = new BigDouble(1.000000000001, 100);

        Assert.That(a == b, Is.False, "13번째 자리 차이 감지");
        Assert.That(a < b, Is.True);
    }

    [Test]
    public void Precision_UltraTiny_VariousMantissa()
    {
        // 다양한 mantissa에서 미세 차이
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
        // 1 근처에서 가장 작은 표현 가능한 차이
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
        // 문자열에서 극미세 차이
        BigDouble a = new BigDouble("1.234567890123456e100");
        BigDouble b = new BigDouble("1.234567890123457e100");

        Assert.That(a.Mantissa, Is.Not.EqualTo(b.Mantissa));
        Assert.That(a == b, Is.False, "문자열 파싱 극미세 차이");
        Assert.That(a < b, Is.True);
    }

    [Test]
    public void Precision_UltraTiny_Sequential()
    {
        // 연속적인 극미세 값들
        double baseVal = 1.0;
        var values = new List<BigDouble>();

        for (int i = 0; i < 10; i++)
        {
            double val = BitConverter.Int64BitsToDouble(
                BitConverter.DoubleToInt64Bits(baseVal) + i
            );
            values.Add(new BigDouble(val, 100));
        }

        // 모든 연속 값이 순서대로 정렬되어야 함
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
        // 큰 지수에서도 극미세 차이 감지
        BigDouble a = new BigDouble(1.000000000000000, 999999);
        BigDouble b = new BigDouble(1.000000000000001, 999999);

        Assert.That(a == b, Is.False, "큰 지수에서도 극미세 차이 감지");
        Assert.That(a < b, Is.True);
    }

    [Test]
    public void Precision_UltraTiny_SmallExponent()
    {
        // 작은 지수에서도 극미세 차이 감지
        BigDouble a = new BigDouble(1.000000000000000, -999999);
        BigDouble b = new BigDouble(1.000000000000001, -999999);

        Assert.That(a == b, Is.False, "작은 지수에서도 극미세 차이 감지");
        Assert.That(a < b, Is.True);
    }

    [Test]
    public void Precision_UltraTiny_NegativeValues()
    {
        // 음수에서 극미세 차이
        BigDouble a = new BigDouble(-1.000000000000001, 100);
        BigDouble b = new BigDouble(-1.000000000000000, 100);

        Assert.That(a == b, Is.False, "음수에서 극미세 차이");
        Assert.That(a < b, Is.True, "-1.000000000000001 < -1.0");
        Assert.That(b > a, Is.True);
    }

    [Test]
    public void Precision_UltraTiny_Arithmetic()
    {
        // 연산 후에도 충분히 큰 차이는 유지
        // 주의: 극미세 차이(1e-15 이하)는 덧셈 정규화로 사라질 수 있음
        BigDouble a = new BigDouble(1.00000000000, 100);
        BigDouble b = new BigDouble(1.00000000001, 100); // 1e-11 차이 (충분히 큼)

        BigDouble sum_a = a + new BigDouble(1, 100);
        BigDouble sum_b = b + new BigDouble(1, 100);

        // 1e-11 정도 차이는 덧셈 후에도 유지되어야 함
        Assert.That(sum_a == sum_b, Is.False, "덧셈 후 차이 유지");
        Assert.That(sum_a < sum_b, Is.True);
    }

    [Test]
    public void Precision_UltraTiny_GameScenario_CriticalDamage()
    {
        // 게임: 크리티컬 데미지 판정 (0.0000000000001% 차이로 승패)
        BigDouble bossHP = new BigDouble(1.000000000000000, 1000);
        BigDouble damage1 = new BigDouble(0.999999999999999, 1000);
        BigDouble damage2 = new BigDouble(1.000000000000000, 1000);

        Assert.That(damage1 < bossHP, Is.True, "데미지1은 보스HP 미달");
        Assert.That(damage2 >= bossHP, Is.True, "데미지2는 보스HP 이상");
        Assert.That(damage1 == damage2, Is.False, "두 데미지는 다름");
    }
}
