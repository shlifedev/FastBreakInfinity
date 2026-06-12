using LD.Numeric.IdleNumber;

namespace LD.Numeric.Test;

/// <summary>
/// BigDouble 확장 수학 함수 테스트 — Abs/Negate/Sign/Reciprocate/Cbrt/Exp/Factorial/
/// 쌍곡함수/로그 변형/Pow 엣지/증감 연산자
/// </summary>
public class ExtendedMathTests
{
    // ===== Abs / Negate / Sign =====

    [Test]
    public void Abs_NegativeAndPositive()
    {
        Assert.That(BigDouble.Abs(new BigDouble(-5, 100)) == new BigDouble(5, 100), Is.True);
        Assert.That(BigDouble.Abs(new BigDouble(5, 100)) == new BigDouble(5, 100), Is.True);
        Assert.That(BigDouble.Abs(BigDouble.Zero) == BigDouble.Zero, Is.True);
    }

    [Test]
    public void Negate_FlipsSign()
    {
        Assert.That(BigDouble.Negate(new BigDouble(5, 100)) == new BigDouble(-5, 100), Is.True);
        Assert.That(-new BigDouble(-3) == new BigDouble(3), Is.True);
    }

    [Test]
    public void Sign_ReturnsCorrectSign()
    {
        Assert.That(BigDouble.Sign(new BigDouble(5, 100)), Is.EqualTo(1));
        Assert.That(BigDouble.Sign(new BigDouble(-5, 100)), Is.EqualTo(-1));
        Assert.That(BigDouble.Sign(BigDouble.Zero), Is.EqualTo(0));
    }

    // ===== Reciprocate =====

    [Test]
    public void Reciprocate_Basic()
    {
        Assert.That(BigDouble.Reciprocate(2).Equals(new BigDouble(0.5), 1e-12), Is.True);
        Assert.That(
            BigDouble.Reciprocate(new BigDouble(4, 2)).Equals(new BigDouble(2.5, -3), 1e-12),
            Is.True
        );
    }

    [Test]
    public void Reciprocate_HugeExponent()
    {
        var result = BigDouble.Reciprocate(new BigDouble(2, 1000));
        Assert.That(
            result.Equals(new BigDouble(5, -1001), 1e-12),
            Is.True,
            $"실제 {result.ToStringMantissaExponent()}"
        );
    }

    // ===== 증감 연산자 =====

    [Test]
    public void IncrementDecrement_Operators()
    {
        BigDouble a = 5;
        a++;
        Assert.That(a == new BigDouble(6), Is.True);
        a--;
        a--;
        Assert.That(a == new BigDouble(4), Is.True);
    }

    // ===== Pow 엣지 =====

    [Test]
    public void Pow_IntegerPower_Exact()
    {
        Assert.That(BigDouble.Pow(2, 10) == new BigDouble(1024), Is.True);
        Assert.That(BigDouble.Pow(new BigDouble(1, 100), 2) == new BigDouble(1, 200), Is.True);
    }

    [Test]
    public void Pow_NegativeBase_IntegerPower()
    {
        Assert.That(BigDouble.Pow(-2, 3.0).Equals(new BigDouble(-8), 1e-12), Is.True);
        Assert.That(BigDouble.Pow(-2, 2.0).Equals(new BigDouble(4), 1e-12), Is.True);
    }

    [Test]
    public void Pow_NegativeBase_FractionalPower_ReturnsNaN()
    {
        Assert.That(BigDouble.IsNaN(BigDouble.Pow(-4, 0.5)), Is.True);
    }

    [Test]
    public void Pow_FractionalPower()
    {
        Assert.That(BigDouble.Pow(10, 0.5).Equals(BigDouble.Sqrt(10), 1e-12), Is.True);
        Assert.That(
            BigDouble.Pow(new BigDouble(1, 100), 0.5).Equals(new BigDouble(1, 50), 1e-9),
            Is.True
        );
    }

    [Test]
    public void Pow_ZeroPower_ReturnsOne()
    {
        Assert.That(BigDouble.Pow(new BigDouble(7, 77), 0L) == BigDouble.One, Is.True);
    }

    [Test]
    public void Pow_ExponentOverflow_ReturnsInfinityOrZero()
    {
        var huge = BigDouble.Pow10(5_000_000_000_000_000_000L);
        Assert.That(BigDouble.IsPositiveInfinity(BigDouble.Pow(huge, 10L)), Is.True);

        var tiny = BigDouble.Pow10(-5_000_000_000_000_000_000L);
        Assert.That(BigDouble.Pow(tiny, 10L) == BigDouble.Zero, Is.True);
    }

    [Test]
    public void Multiply_ExponentOverflow_ReturnsInfinityOrZero()
    {
        var huge = BigDouble.Pow10(9_000_000_000_000_000_000L);
        Assert.That(BigDouble.IsPositiveInfinity(huge * huge), Is.True);

        var tiny = BigDouble.Pow10(-9_000_000_000_000_000_000L);
        Assert.That(tiny * tiny == BigDouble.Zero, Is.True);
    }

    [Test]
    public void Pow10_FractionalPower()
    {
        Assert.That(BigDouble.Pow10(0.5).Equals(BigDouble.Sqrt(10), 1e-12), Is.True);
        Assert.That(BigDouble.Pow10(100.0) == new BigDouble(1, 100), Is.True);
    }

    // ===== Sqrt / Cbrt =====

    [Test]
    public void Sqrt_NegativeValue_ReturnsNaN()
    {
        Assert.That(BigDouble.IsNaN(BigDouble.Sqrt(-4)), Is.True);
    }

    [Test]
    public void Sqrt_HugeValue_OddExponent()
    {
        // sqrt(1e101) = 10^50.5 ≈ 3.162e50
        var result = BigDouble.Sqrt(new BigDouble(1, 101));
        Assert.That(result.Equals(new BigDouble(3.16227766016838, 50), 1e-9), Is.True);
    }

    [Test]
    public void Cbrt_Basic()
    {
        Assert.That(BigDouble.Cbrt(27).Equals(new BigDouble(3), 1e-12), Is.True);
        Assert.That(BigDouble.Cbrt(-27).Equals(new BigDouble(-3), 1e-12), Is.True);
        Assert.That(BigDouble.Cbrt(new BigDouble(1, 3)).Equals(new BigDouble(10), 1e-12), Is.True);
    }

    [Test]
    public void Cbrt_HugeValue_ExponentModBranches()
    {
        // 지수 mod 3이 0/1/2인 케이스를 모두 — log10 기준으로 검증
        Assert.That(
            BigDouble.Log10(BigDouble.Cbrt(new BigDouble(1, 99))),
            Is.EqualTo(33.0).Within(1e-9)
        );
        Assert.That(
            BigDouble.Log10(BigDouble.Cbrt(new BigDouble(1, 100))),
            Is.EqualTo(100 / 3.0).Within(1e-9)
        );
        Assert.That(
            BigDouble.Log10(BigDouble.Cbrt(new BigDouble(1, 101))),
            Is.EqualTo(101 / 3.0).Within(1e-9)
        );
    }

    // ===== Exp / Factorial =====

    [Test]
    public void Exp_Basic()
    {
        Assert.That(BigDouble.Exp(1).Equals(new BigDouble(Math.E), 1e-9), Is.True);
        Assert.That(BigDouble.Exp(0).Equals(BigDouble.One, 1e-9), Is.True);
    }

    [Test]
    public void Exp_LargeValue_MatchesLog()
    {
        // exp(1000)의 자연로그는 1000이어야 함
        var result = BigDouble.Exp(1000);
        Assert.That(BigDouble.Ln(result), Is.EqualTo(1000.0).Within(1e-6));
    }

    [Test]
    public void Factorial_SmallValues_NearExact()
    {
        // 스털링 근사라 정확하지 않음 — 상대 오차로 검증
        Assert.That(BigDouble.Factorial(5).Equals(new BigDouble(120), 1e-6), Is.True);
        Assert.That(BigDouble.Factorial(10).Equals(new BigDouble(3628800), 1e-6), Is.True);
    }

    [Test]
    public void Factorial_LargeValue_MatchesStirlingMagnitude()
    {
        // 100! ≈ 9.33e157
        var result = BigDouble.Factorial(100);
        Assert.That(BigDouble.Log10(result), Is.EqualTo(157.97).Within(0.01));
    }

    [Test]
    public void Factorial_ResultExponentOverflow_ReturnsPositiveInfinity()
    {
        // n=1e30이면 log10(n!) ≈ 3e31로 long 지수 범위를 초과 — 포화된 가짜 값이 아니라 Infinity여야 함
        var result = BigDouble.Factorial(new BigDouble(1, 30));
        Assert.That(BigDouble.IsPositiveInfinity(result), Is.True, $"실제 {result}");
    }

    [Test]
    public void Factorial_BeyondDoubleRange_ReturnsPositiveInfinity()
    {
        // double로 변환 불가능한 입력(1e400)은 NaN으로 붕괴하지 않고 Infinity여야 함
        var result = BigDouble.Factorial(new BigDouble(1, 400));
        Assert.That(BigDouble.IsPositiveInfinity(result), Is.True, $"실제 {result}");
    }

    // ===== 쌍곡함수 =====

    [Test]
    public void Hyperbolic_BasicValues()
    {
        Assert.That(BigDouble.Sinh(1).Equals(new BigDouble(Math.Sinh(1)), 1e-9), Is.True);
        Assert.That(BigDouble.Cosh(1).Equals(new BigDouble(Math.Cosh(1)), 1e-9), Is.True);
        Assert.That(BigDouble.Tanh(1).Equals(new BigDouble(Math.Tanh(1)), 1e-9), Is.True);
    }

    [Test]
    public void InverseHyperbolic_BasicValues()
    {
        Assert.That(BigDouble.Asinh(1), Is.EqualTo(Math.Asinh(1)).Within(1e-9));
        Assert.That(BigDouble.Acosh(2), Is.EqualTo(Math.Acosh(2)).Within(1e-9));
        Assert.That(BigDouble.Atanh(0.5), Is.EqualTo(Math.Atanh(0.5)).Within(1e-9));
    }

    [Test]
    public void Atanh_OutOfDomain_ReturnsNaN()
    {
        Assert.That(double.IsNaN(BigDouble.Atanh(1)), Is.True);
        Assert.That(double.IsNaN(BigDouble.Atanh(-2)), Is.True);
    }

    // ===== 로그 변형 =====

    [Test]
    public void Log10_OutOfDomain_FollowsIeeeSemantics()
    {
        // 음수는 NaN, 0은 -Infinity — Math.Log10과 동일한 의미를 유지해야 한다
        Assert.That(double.IsNaN(BigDouble.Log10(new BigDouble(-5))), Is.True);
        Assert.That(double.IsNegativeInfinity(BigDouble.Log10(BigDouble.Zero)), Is.True);
    }

    [Test]
    public void Log10_HugeValue()
    {
        Assert.That(BigDouble.Log10(new BigDouble(1, 100)), Is.EqualTo(100.0).Within(1e-12));
        Assert.That(
            BigDouble.Log10(new BigDouble(5, 100)),
            Is.EqualTo(100.0 + Math.Log10(5)).Within(1e-12)
        );
    }

    [Test]
    public void AbsLog10_NegativeValue()
    {
        Assert.That(BigDouble.AbsLog10(new BigDouble(-1, 50)), Is.EqualTo(50.0).Within(1e-12));
    }

    [Test]
    public void Log2_And_Ln()
    {
        Assert.That(BigDouble.Log2(8), Is.EqualTo(3.0).Within(1e-9));
        Assert.That(BigDouble.Ln(new BigDouble(Math.E)), Is.EqualTo(1.0).Within(1e-9));
    }

    [Test]
    public void Log_ArbitraryBase()
    {
        Assert.That(BigDouble.Log(100, 10.0), Is.EqualTo(2.0).Within(1e-9));
        Assert.That(BigDouble.Log(8, 2.0), Is.EqualTo(3.0).Within(1e-9));
        Assert.That(
            BigDouble.Log(new BigDouble(1, 100), new BigDouble(10)),
            Is.EqualTo(100.0).Within(1e-9)
        );
    }

    [Test]
    public void Log_BaseZero_ReturnsNaN()
    {
        Assert.That(double.IsNaN(BigDouble.Log(5, 0.0)), Is.True);
    }

    // ===== Min / Max NaN 처리 =====

    [Test]
    public void MinMax_WithNaN_ReturnsNaN()
    {
        Assert.That(BigDouble.IsNaN(BigDouble.Max(BigDouble.NaN, 5)), Is.True);
        Assert.That(BigDouble.IsNaN(BigDouble.Max(5, BigDouble.NaN)), Is.True);
        Assert.That(BigDouble.IsNaN(BigDouble.Min(BigDouble.NaN, 5)), Is.True);
        Assert.That(BigDouble.IsNaN(BigDouble.Min(5, BigDouble.NaN)), Is.True);
    }

    // ===== 확장 메서드 =====

    [Test]
    public void ExtensionMethods_MatchStaticMethods()
    {
        BigDouble value = new BigDouble(2, 50);
        Assert.That(value.Sqr() == BigDouble.Pow(value, 2L), Is.True);
        Assert.That(value.Abs() == BigDouble.Abs(value), Is.True);
        Assert.That(value.Sqrt() == BigDouble.Sqrt(value), Is.True);
        Assert.That(value.Reciprocate() == BigDouble.Reciprocate(value), Is.True);
        Assert.That(value.Sign(), Is.EqualTo(1));
    }
}
