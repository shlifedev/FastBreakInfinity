using LD.Numeric.IdleNumber;

namespace LD.Numeric.Test;

/// <summary>
/// BigValueInfo.ExponentFormatToBigValueInfo 직접 테스트 — e 표기법 수동 파서
/// </summary>
public class BigValueInfoTests
{
    [Test]
    public void Parse_BasicExponentFormat()
    {
        var info = BigValueInfo.ExponentFormatToBigValueInfo("1.5e3");
        Assert.That(info.Mantissa, Is.EqualTo(1.5).Within(1e-15));
        Assert.That(info.Exponent, Is.EqualTo(3L));
    }

    [Test]
    public void Parse_UppercaseE()
    {
        var info = BigValueInfo.ExponentFormatToBigValueInfo("2E10");
        Assert.That(info.Mantissa, Is.EqualTo(2.0).Within(1e-15));
        Assert.That(info.Exponent, Is.EqualTo(10L));
    }

    [Test]
    public void Parse_NegativeMantissaAndExponent()
    {
        var info = BigValueInfo.ExponentFormatToBigValueInfo("-2.5e-10");
        Assert.That(info.Mantissa, Is.EqualTo(-2.5).Within(1e-15));
        Assert.That(info.Exponent, Is.EqualTo(-10L));
    }

    [Test]
    public void Parse_ExplicitPlusSigns()
    {
        var info = BigValueInfo.ExponentFormatToBigValueInfo("+3e+5");
        Assert.That(info.Mantissa, Is.EqualTo(3.0).Within(1e-15));
        Assert.That(info.Exponent, Is.EqualTo(5L));
    }

    [Test]
    public void Parse_NoExponentPart_ExponentIsZero()
    {
        var info = BigValueInfo.ExponentFormatToBigValueInfo("42");
        Assert.That(info.Mantissa, Is.EqualTo(42.0));
        Assert.That(info.Exponent, Is.EqualTo(0L));

        info = BigValueInfo.ExponentFormatToBigValueInfo("1.25");
        Assert.That(info.Mantissa, Is.EqualTo(1.25).Within(1e-15));
        Assert.That(info.Exponent, Is.EqualTo(0L));
    }

    [Test]
    public void Parse_VeryLargeExponent()
    {
        var info = BigValueInfo.ExponentFormatToBigValueInfo("1e999999999");
        Assert.That(info.Exponent, Is.EqualTo(999999999L));
    }

    [TestCase("")]
    [TestCase(null)]
    [TestCase("e5")] // 가수 없음
    [TestCase("1e")] // 지수 숫자 없음
    [TestCase("1.2.3")] // 소수점 중복
    [TestCase("1e5e3")] // e 중복
    [TestCase("1e2.5")] // 지수에 소수점
    [TestCase("1-2e5")] // 부호가 가운데
    [TestCase("12x")] // 숫자가 아닌 문자
    [TestCase("1e92233720368547758079")] // long 오버플로
    public void Parse_InvalidInput_ThrowsFormatException(string? input)
    {
        Assert.Throws<FormatException>(() => BigValueInfo.ExponentFormatToBigValueInfo(input!));
    }
}
