using LD.Numeric.IdleNumber;

namespace LD.Numeric.Test;

/// <summary>
/// 전수 버그 헌트(2차 리뷰)에서 발견된 버그들의 회귀 테스트
/// </summary>
public class BugHuntRegressionTests
{
    // ===== H1: Cbrt가 3의 배수가 아닌 음수 지수에서 보정 상수(10^(1/3), 10^(2/3))를
    //           뒤바꿔 곱하던 버그 — C# %의 음수 나머지를 잉여류로 오용 =====

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

    // ===== H2: Normalize가 subnormal 구간(약 1e-323~1e-309)에서 Log10 오차로
    //           mantissa >= 10인 비정규 값을 만들어 ToDouble이 한 자릿수 어긋남 =====

    [Test]
    public void Normalize_Subnormal_KeepsMantissaInvariant()
    {
        var a = new BigDouble(1e-323);
        Assert.That(a.Mantissa, Is.GreaterThanOrEqualTo(1).And.LessThan(10));
        Assert.That(a.ToDouble(), Is.EqualTo(1e-323));

        var b = new BigDouble(1e-322);
        Assert.That(b.Mantissa, Is.GreaterThanOrEqualTo(1).And.LessThan(10));
        Assert.That(b.ToDouble(), Is.EqualTo(1e-322));

        // 기존 특수처리(최소 subnormal) 회귀 확인
        Assert.That(new BigDouble(5e-324).ToDouble(), Is.EqualTo(5e-324));
    }

    // ===== H3/H4: NaN의 지수가 long.MinValue 센티널이라 지수 연산 오버플로 핸들러가
    //              NaN을 Zero/Infinity로 둔갑시키던 버그 =====

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

    // ===== H5: Pow(음수 밑, 큰 홀수 long 지수)에서 mantissa^power 오버플로 시
    //           제곱 후 재귀하면서 부호가 사라지던 버그 =====

    [Test]
    public void Pow_NegativeBase_LargeOddPower_KeepsSign()
    {
        var result = BigDouble.Pow(new BigDouble(-9), 401L);
        Assert.That(result.Mantissa, Is.LessThan(0));
        Assert.That(result.Exponent, Is.EqualTo(382));
        Assert.That(result.Mantissa, Is.EqualTo(-4.4796727106).Within(1e-4));

        // 짝수 지수는 양수 유지
        Assert.That(BigDouble.Pow(new BigDouble(-9), 400L).Mantissa, Is.GreaterThan(0));
    }

    // ===== H6: Log(x, base=1)이 0으로 나누기를 타고 Infinity 반환 (Math.Log는 NaN) =====

    [Test]
    public void Log_BaseOne_IsNaN()
    {
        Assert.That(double.IsNaN(BigDouble.Log(new BigDouble(100), 1.0)), Is.True);
    }

    // ===== H7: Equals는 모든 zero를 같다고 보는데 GetHashCode는 지수를 섞어
    //           해시 계약(같으면 해시도 같다)을 위반 =====

    [Test]
    public void GetHashCode_AllZerosShareHash()
    {
        var z1 = BigDouble.Zero;
        var z2 = BigDouble.FromMantissaExponentNoNormalize(0, 5);
        Assert.That(z1.Equals(z2), Is.True);
        Assert.That(z1.GetHashCode(), Is.EqualTo(z2.GetHashCode()));
    }

    // ===== H8: ToString에서 가수 반올림이 1000에 도달해도 알파벳 단위가 안 올라가
    //           "1000G"(가수 4자리) 표기가 나오던 버그 =====

    [Test]
    public void ToString_MantissaRoundingCarriesUnit()
    {
        Assert.That(new BigDouble("9.999999e23").ToString(), Is.EqualTo("1.00H"));
    }

    // ===== H9: ConvertToAlphabetUnit의 F-포맷 반올림이 1000으로 올라가도 단위 미갱신 =====

    [Test]
    public void ConvertToAlphabetUnit_RoundingCarriesUnit()
    {
        Assert.That(999999.0.ConvertToAlphabetUnit(), Is.EqualTo("1.00B"));
        Assert.That(999994.0.ConvertToAlphabetUnit(), Is.EqualTo("999.99A"));
    }

    // ===== H10: 음수 값에 알파벳 단위가 전혀 안 붙던 버그 =====

    [Test]
    public void ConvertToAlphabetUnit_NegativeValues()
    {
        Assert.That((-5000.0).ConvertToAlphabetUnit(), Is.EqualTo("-5.00A"));
        Assert.That((-1500000.0).ConvertToAlphabetUnit(), Is.EqualTo("-1.50B"));
        Assert.That((-500.0).ConvertToAlphabetUnit(), Is.EqualTo("-500"));
    }

    // ===== H11: 소문자 단위가 c-'A' 계산을 그대로 타서 "1.5a"가 1.5e99로 둔갑 =====

    [Test]
    public void ConvertFromAlphabetUnit_LowercaseUnit_Throws()
    {
        Assert.Throws<FormatException>(() => AlphabetConverter.ConvertFromAlphabetUnit("1.5a"));
    }

    // ===== H12: AdjustedMantissa가 음수 지수에서 3자리 보정을 적용해 0.5를 500으로 =====

    [Test]
    public void AdjustedMantissa_NegativeExponent_ReturnsActualValue()
    {
        Assert.That(new BigDouble(0.5).AdjustedMantissa(), Is.EqualTo(0.5));
    }

    // ===== H13/H14: GetDigits의 int 캐스팅 포화 — 큰 음수는 크래시, 21억 초과는 오답 =====

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
        // 기존 동작 회귀 확인
        Assert.That(NumberUtility.GetDigits(999.0), Is.EqualTo(3));
        Assert.That(NumberUtility.GetDigits(0.5), Is.EqualTo(0));
    }
}
