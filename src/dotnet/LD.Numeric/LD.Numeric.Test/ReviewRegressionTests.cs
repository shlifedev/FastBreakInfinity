using System.Globalization;
using LD.Numeric.IdleNumber;

namespace LD.Numeric.Test;

/// <summary>
/// 전체 코드 리뷰에서 발견된 버그들의 회귀 테스트
/// </summary>
public class ReviewRegressionTests
{
    // ===== C1: 1 미만 값(음수 지수)의 ToString이 값을 최대 1000배로 표시하던 버그 =====

    [Test]
    public void ToString_FractionalValues()
    {
        Assert.That(new BigDouble(0.5).ToString(), Is.EqualTo("0.5"));
        Assert.That(new BigDouble(0.05).ToString(), Is.EqualTo("0.05"));
        Assert.That(new BigDouble(0.001).ToString(), Is.EqualTo("0.001"));
        Assert.That(new BigDouble(0.25).ToString(), Is.EqualTo("0.25"));
        Assert.That(new BigDouble(-0.5).ToString(), Is.EqualTo("-0.5"));
    }

    // ===== C2: ToFloat()이 지수를 무시하고 mantissa만 반환하던 버그 =====

    [Test]
    public void ToFloat_UsesExponent()
    {
        Assert.That(new BigDouble(5e10).ToFloat(), Is.EqualTo(5e10f));
        Assert.That(new BigDouble(1234).ToFloat(), Is.EqualTo(1234f));
        Assert.That(new BigDouble(-2.5e5).ToFloat(), Is.EqualTo(-2.5e5f));
        Assert.That(new BigDouble(1, 100).ToFloat(), Is.EqualTo(float.PositiveInfinity));
    }

    // ===== C3: Is10이 -10도 10으로 판정해 Pow(-10, n)이 양수가 되던 버그 =====

    [Test]
    public void Pow_NegativeTen_KeepsSign()
    {
        Assert.That(BigDouble.Pow(new BigDouble(-10), 3L) == new BigDouble(-1000), Is.True);
        Assert.That(BigDouble.Pow(new BigDouble(-10), 2L) == new BigDouble(100), Is.True);
        Assert.That(BigDouble.Pow(new BigDouble(-10), 3.0) == new BigDouble(-1000), Is.True);
    }

    // ===== C4: OptimizeToString 소수부 절삭/캐리 버그 =====

    [Test]
    public void OptimizeToString_RoundsFractionalPart()
    {
        Assert.That(1.567.OptimizeToString(1), Is.EqualTo("1.6"));
        Assert.That(0.494.OptimizeToString(2), Is.EqualTo("0.49"));
    }

    [Test]
    public void OptimizeToString_CarriesIntoIntegerPart()
    {
        Assert.That(0.999999999.OptimizeToString(2), Is.EqualTo("1.00"));
        Assert.That(1.999.OptimizeToString(2), Is.EqualTo("2.00"));
        Assert.That((-1.999).OptimizeToString(2), Is.EqualTo("-2.00"));
    }

    [Test]
    public void OptimizeToString_BeyondLongRange_FallsBack()
    {
        Assert.That(
            1e19.OptimizeToString(2),
            Is.EqualTo(1e19.ToString("F2", CultureInfo.InvariantCulture))
        );
    }

    // ===== M1: PowersOf10 테이블이 Math.Pow 기반 값으로 채워지던 문제 =====

    [Test]
    public void PowersOf10_MatchCorrectlyRoundedValues()
    {
        for (long e = -323; e <= 308; e++)
        {
            double expected = double.Parse("1e" + e, CultureInfo.InvariantCulture);
            Assert.That(new BigDouble(1, e).ToDouble(), Is.EqualTo(expected), $"1e{e}");
        }
    }

    // ===== M3: Math.Log10 경계 오차로 알파벳 단위가 한 단계 어긋나던 문제 =====

    [Test]
    public void AlphabetConverter_PowerOfTenBoundary()
    {
        Assert.That(AlphabetConverter.GetExponent(1e21), Is.EqualTo(21));
        Assert.That(1e21.ConvertToAlphabetUnit(), Is.EqualTo("1.00G"));
    }

    // ===== M4: e-포맷 파서가 비정상 입력을 묵인하던 문제 =====

    [Test]
    public void Parse_InvalidExponentFormat_Throws()
    {
        Assert.Throws<FormatException>(() => BigDouble.Parse("1e5e3"));
        Assert.Throws<FormatException>(() => BigDouble.Parse("1x2e5"));
        Assert.Throws<FormatException>(() => BigDouble.Parse("1-2e5"));
        Assert.Throws<FormatException>(() => BigDouble.Parse("1e92233720368547758079"));
    }

    [Test]
    public void ParseDouble_HugeExponent_SaturatesToInfinity()
    {
        // 지수부 int 오버플로 랩어라운드로 1.0이 반환되던 케이스
        Assert.That(FastDouble.ParseDouble("1e8589934592"), Is.EqualTo(double.PositiveInfinity));
    }

    // ===== M5: 7글자 이상 알파벳 단위에서 (int)Math.Pow(26, n) 오버플로 =====

    [Test]
    public void GetExponentFromAlphabetUnit_LongUnit_NoOverflow()
    {
        // H*26^6 항이 int.MaxValue를 넘는 케이스
        var range = BigDouble.GetExponentFromAlphabetUnit("HAAAAAA");
        Assert.That(range.rangeA, Is.EqualTo(7451048517L));
        Assert.That(range.rangeB, Is.EqualTo(7451048519L));

        // 기존 동작 회귀 확인
        Assert.That(BigDouble.GetExponentFromAlphabetUnit("A"), Is.EqualTo((3L, 5L)));
        Assert.That(BigDouble.GetExponentFromAlphabetUnit("B"), Is.EqualTo((6L, 8L)));
    }
}
