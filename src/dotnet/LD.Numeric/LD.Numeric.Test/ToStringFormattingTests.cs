using LD.Numeric.IdleNumber;

namespace LD.Numeric.Test;

/// <summary>
/// BigDouble.ToString 포맷 규칙 테스트 — 자릿수별 소수점 표시와 알파벳 단위
/// </summary>
public class ToStringFormattingTests
{
    [Test]
    public void ToString_Zero()
    {
        Assert.That(BigDouble.Zero.ToString(), Is.EqualTo("0"));
    }

    [Test]
    public void ToString_SmallIntegers_NoDecimals()
    {
        // 지수 2 이하 + 가수 한 자리면 소수점 없이 표시
        Assert.That(new BigDouble(7).ToString(), Is.EqualTo("7"));
        Assert.That(new BigDouble(12).ToString(), Is.EqualTo("12"));
        Assert.That(new BigDouble(999).ToString(), Is.EqualTo("999"));
    }

    [Test]
    public void ToString_ThousandsRange_TwoDecimalsWithUnit()
    {
        // 조정 가수가 한 자리(1~9)면 소수 2자리
        Assert.That(new BigDouble(1234).ToString(), Is.EqualTo("1.23A"));
        Assert.That(new BigDouble(5000).ToString(), Is.EqualTo("5.00A"));
    }

    [Test]
    public void ToString_TenThousandsRange_OneDecimalWithUnit()
    {
        // 조정 가수가 두 자리(10~99)면 소수 1자리
        Assert.That(new BigDouble(12345).ToString(), Is.EqualTo("12.3A"));
    }

    [Test]
    public void ToString_HundredThousandsRange_NoDecimalsWithUnit()
    {
        // 조정 가수가 세 자리(100~999)면 소수점 없음
        Assert.That(new BigDouble(123456).ToString(), Is.EqualTo("123A"));
    }

    [Test]
    public void ToString_MillionRange_NextUnit()
    {
        Assert.That(new BigDouble(1, 6).ToString(), Is.EqualTo("1.00B"));
        Assert.That(new BigDouble(2.5, 9).ToString(), Is.EqualTo("2.50C"));
    }

    [Test]
    public void ToString_NegativeValue_KeepsSign()
    {
        Assert.That(new BigDouble(-1234).ToString(), Is.EqualTo("-1.23A"));
        Assert.That(new BigDouble(-12).ToString(), Is.EqualTo("-12"));
    }

    [Test]
    public void ToString_LessThanOne_PlainDoubleFormat()
    {
        Assert.That(new BigDouble(0.5).ToString(), Is.EqualTo("0.5"));
        Assert.That(new BigDouble(0.001).ToString(), Is.EqualTo("0.001"));
    }

    [Test]
    public void ToString_HugeExponent_UsesLongAlphabetUnit()
    {
        // 지수 1000 → index 1000/3-1 = 332
        var expected = AlphabetManager.GetAlphabetUnit(1000 / 3 - 1);
        var str = new BigDouble(1.5, 1000).ToString();
        Assert.That(str, Does.EndWith(expected));
        Assert.That(str, Does.StartWith("1"));
    }

    [Test]
    public void ToString_MantissaRoundsToThreeDecimals()
    {
        // 가수는 소수 3자리로 반올림된 뒤 포맷됨
        Assert.That(new BigDouble(1.2341234, 3).ToString(), Is.EqualTo("1.23A"));
        Assert.That(new BigDouble(1.9991234, 3).ToString(), Is.EqualTo("2.00A"));
    }
}
