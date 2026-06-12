using System.Globalization;
using LD.Numeric.IdleNumber;

namespace LD.Numeric.Test;

/// <summary>
/// FastDouble 집중 리뷰에서 발견된 버그들의 회귀 테스트
/// </summary>
public class FastDoubleReviewTests
{
    // ===== F1: maxDecimalPlaces 절삭이 지수 스케일링 전에 적용돼
    //           지수 표기에서 정수부 유효숫자까지 깎이던 버그 =====

    [Test]
    public void ParseDouble_ExponentNotation_KeepsFullPrecision()
    {
        // 선행 0이 maxDecimalPlaces를 소진해 0이 반환되던 케이스
        Assert.That(FastDouble.ParseDouble("0.0000001234567e10"), Is.EqualTo(1234.567));
        // 최종 값에 소수부가 없는데도 12,345,670,000으로 깎이던 케이스
        Assert.That(FastDouble.ParseDouble("1.23456789e10"), Is.EqualTo(12345678900.0));
        Assert.That(FastDouble.ParseDouble("1.23456789e-2"), Is.EqualTo(0.0123456789));
    }

    [Test]
    public void ParseDouble_PlainNotation_StillTruncates()
    {
        // 평문 십진수의 소수부 절삭(반올림 아님)은 의도된 설계
        Assert.That(FastDouble.ParseDouble("1.9999999", 6), Is.EqualTo(1.999999));
        Assert.That(FastDouble.ParseDouble("0.0000001", 6), Is.EqualTo(0));
    }

    // ===== F2: 숫자 없는 입력이 조용히 0/-0/1로 파싱되던 버그 =====

    [Test]
    public void ParseDouble_NoMantissaDigits_Throws()
    {
        Assert.Throws<FormatException>(() => FastDouble.ParseDouble("-"));
        Assert.Throws<FormatException>(() => FastDouble.ParseDouble("+"));
        Assert.Throws<FormatException>(() => FastDouble.ParseDouble("."));
    }

    [Test]
    public void ParseDouble_ExponentWithoutDigits_Throws()
    {
        Assert.Throws<FormatException>(() => FastDouble.ParseDouble("1e"));
        Assert.Throws<FormatException>(() => FastDouble.ParseDouble("1e-"));
        Assert.Throws<FormatException>(() => FastDouble.ParseDouble("1e+"));
    }

    [Test]
    public void ParseDouble_NullOrEmpty_ReturnsZero()
    {
        // 기존 호환 동작 유지
        Assert.That(FastDouble.ParseDouble(""), Is.EqualTo(0));
        Assert.That(FastDouble.ParseDouble(null), Is.EqualTo(0));
    }

    // ===== F3: OptimizeToString(0)이 culture 의존적인 느린 표준 포맷터를 타던 문제 =====

    [Test]
    public void OptimizeToString_ZeroDecimalPlaces_UsesFastPath()
    {
        Assert.That(1234.567.OptimizeToString(0), Is.EqualTo("1235"));
        Assert.That((-1234.6).OptimizeToString(0), Is.EqualTo("-1235"));
        Assert.That(0.0.OptimizeToString(0), Is.EqualTo("0"));
        // 기존엔 ToString("0")이 "∞"를 반환해 다른 경로("Infinity")와 표기가 갈렸음
        Assert.That(double.PositiveInfinity.OptimizeToString(0), Is.EqualTo("Infinity"));
        Assert.That(double.NaN.OptimizeToString(0), Is.EqualTo("NaN"));
    }

    // ===== F4: 소수부가 banker's rounding이라 정수부 경로와 반올림 방식이 갈리던 문제 =====

    [Test]
    public void OptimizeToString_MidpointRoundsAwayFromZero()
    {
        Assert.That(0.125.OptimizeToString(2), Is.EqualTo("0.13"));
        Assert.That(0.5.OptimizeToString(0), Is.EqualTo("1"));
        Assert.That(2.5.OptimizeToString(0), Is.EqualTo("3"));
    }

    [Test]
    public void OptimizeToString_ZeroValue_PadsDecimals()
    {
        Assert.That(0.0.OptimizeToString(3), Is.EqualTo("0.000"));
        Assert.That((-0.001).OptimizeToString(2), Is.EqualTo("-0.00"));
    }

    // ===== F5: substring 할당 없이 파싱할 수 있는 ReadOnlySpan 오버로드 =====

    [Test]
    public void ParseDouble_SpanOverload()
    {
        Assert.That(FastDouble.ParseDouble("123.45".AsSpan()), Is.EqualTo(123.45));
        Assert.That(FastDouble.ParseDouble("price:1.5e3".AsSpan(6)), Is.EqualTo(1500));
    }
}
