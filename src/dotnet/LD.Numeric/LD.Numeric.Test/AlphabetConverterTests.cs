using System.Numerics;
using LD.Numeric.IdleNumber;

namespace LD.Numeric.Test;

/// <summary>
/// AlphabetConverter 테스트 — 숫자↔알파벳 단위 변환과 역변환
/// </summary>
public class AlphabetConverterTests
{
    // ===== GetExponent =====

    [TestCase(1000.0, 3L)]
    [TestCase(999999.0, 3L)]
    [TestCase(1e6, 6L)]
    [TestCase(1e9, 9L)]
    [TestCase(1e21, 21L)]
    [TestCase(5e10, 9L)]
    public void GetExponent_ReturnsThreeDigitUnitExponent(double value, long expected)
    {
        Assert.That(AlphabetConverter.GetExponent(value), Is.EqualTo(expected));
    }

    [Test]
    public void GetExponent_BigInteger()
    {
        Assert.That(AlphabetConverter.GetExponent(BigInteger.Pow(10, 30)), Is.EqualTo(30L));
        Assert.That(AlphabetConverter.GetExponent(new BigInteger(1000)), Is.EqualTo(3L));
    }

    [Test]
    public void GetExponent_NonPositive_Throws()
    {
        // Log10(0)=-Inf, Log10(음수)=NaN이 long 캐스팅을 거치며 쓰레기 지수가 되는 것 방지.
        // BigInteger 버전(BigInteger.Log10이 throw)과 동작을 맞춘다
        Assert.Throws<ArgumentOutOfRangeException>(() => AlphabetConverter.GetExponent(0.0));
        Assert.Throws<ArgumentOutOfRangeException>(() => AlphabetConverter.GetExponent(-5.0));
        Assert.Throws<ArgumentOutOfRangeException>(() => AlphabetConverter.GetExponent(double.NaN));
    }

    // ===== ConvertToAlphabetUnit =====

    [Test]
    public void ConvertToAlphabetUnit_BelowThousand_ReturnsPlainNumber()
    {
        Assert.That(999.0.ConvertToAlphabetUnit(), Is.EqualTo("999"));
        Assert.That(999.5.ConvertToAlphabetUnit(), Is.EqualTo("999.5"));
        Assert.That(0.0.ConvertToAlphabetUnit(), Is.EqualTo("0"));
    }

    [Test]
    public void ConvertToAlphabetUnit_BasicUnits()
    {
        Assert.That(1000.0.ConvertToAlphabetUnit(), Is.EqualTo("1.00A"));
        Assert.That(1500.0.ConvertToAlphabetUnit(), Is.EqualTo("1.50A"));
        Assert.That(1e6.ConvertToAlphabetUnit(), Is.EqualTo("1.00B"));
        Assert.That(2.5e9.ConvertToAlphabetUnit(), Is.EqualTo("2.50C"));
    }

    [Test]
    public void ConvertToAlphabetUnit_MaxDecimalPoint()
    {
        Assert.That(1234.0.ConvertToAlphabetUnit(0), Is.EqualTo("1A"));
        Assert.That(1234.0.ConvertToAlphabetUnit(1), Is.EqualTo("1.2A"));
        Assert.That(1234.0.ConvertToAlphabetUnit(3), Is.EqualTo("1.234A"));
    }

    [Test]
    public void ConvertToAlphabetUnit_IntAndFloatOverloads()
    {
        Assert.That(2500.ConvertToAlphabetUnit(), Is.EqualTo("2.50A"));
        Assert.That(1000f.ConvertToAlphabetUnit(), Is.EqualTo("1.00A"));
        Assert.That(500.ConvertToAlphabetUnit(), Is.EqualTo("500"));
    }

    // ===== ConvertFromAlphabetUnit (역변환) =====

    [Test]
    public void ConvertFromAlphabetUnit_BasicUnits()
    {
        Assert.That(
            AlphabetConverter.ConvertFromAlphabetUnit("1.5A"),
            Is.EqualTo(1500.0).Within(1e-9)
        );
        Assert.That(AlphabetConverter.ConvertFromAlphabetUnit("2B"), Is.EqualTo(2e6).Within(1e-3));
        Assert.That(
            AlphabetConverter.ConvertFromAlphabetUnit("1AA"),
            Is.EqualTo(1e81).Within(1e72)
        );
    }

    [Test]
    public void ConvertFromAlphabetUnit_PlainNumber_ParsesDirectly()
    {
        Assert.That(AlphabetConverter.ConvertFromAlphabetUnit("123"), Is.EqualTo(123.0));
        Assert.That(AlphabetConverter.ConvertFromAlphabetUnit("123.45"), Is.EqualTo(123.45));
    }

    [Test]
    public void ConvertFromAlphabetUnit_EmptyOrLettersOnly_ReturnsZero()
    {
        Assert.That(AlphabetConverter.ConvertFromAlphabetUnit(""), Is.EqualTo(0.0));
        Assert.That(AlphabetConverter.ConvertFromAlphabetUnit("ABC"), Is.EqualTo(0.0));
    }

    [Test]
    public void ConvertFromAlphabetUnit_RoundTripsWithConvertTo()
    {
        double[] values = { 1000, 1500, 2.5e6, 7.25e9, 3e12, 9.99e15 };
        foreach (var value in values)
        {
            var str = value.ConvertToAlphabetUnit();
            var back = AlphabetConverter.ConvertFromAlphabetUnit(str);
            Assert.That(back, Is.EqualTo(value).Within(value * 0.01), $"'{str}' 라운드트립 실패");
        }
    }

    // ===== BigDouble 쪽 알파벳 단위 API =====

    [Test]
    public void BigDouble_GetExponentFromAlphabetUnit_KnownRanges()
    {
        Assert.That(BigDouble.GetExponentFromAlphabetUnit("A"), Is.EqualTo((3L, 5L)));
        Assert.That(BigDouble.GetExponentFromAlphabetUnit("B"), Is.EqualTo((6L, 8L)));
        Assert.That(BigDouble.GetExponentFromAlphabetUnit("AA"), Is.EqualTo((81L, 83L)));
    }

    [Test]
    public void BigDouble_GetExponentFromAlphabetUnit_Empty_Throws()
    {
        Assert.Throws<ArgumentException>(() => BigDouble.GetExponentFromAlphabetUnit(""));
    }

    [Test]
    public void BigDouble_GetNumberAndExponentFromUnitName()
    {
        Assert.That(BigDouble.GetNumberFromUnitName("A"), Is.EqualTo(1L));
        Assert.That(BigDouble.GetExponentFromUnitName("A"), Is.EqualTo(3L));
        Assert.That(BigDouble.GetNumberFromUnitName("B"), Is.EqualTo(2L));
        Assert.That(BigDouble.GetExponentFromUnitName("B"), Is.EqualTo(6L));
    }

    [Test]
    public void BigDouble_GetAlphabetUnit_FromExponent()
    {
        Assert.That(BigDouble.GetAlphabetUnit(3), Is.EqualTo("A"));
        Assert.That(BigDouble.GetAlphabetUnit(6), Is.EqualTo("B"));
        Assert.That(BigDouble.GetAlphabetUnit(0), Is.EqualTo(string.Empty));
    }
}
