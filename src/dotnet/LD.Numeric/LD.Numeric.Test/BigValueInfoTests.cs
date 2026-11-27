using LD.Numeric.IdleNumber;

namespace LD.Numeric.Test;

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

    [Test]
    public void Parse_FractionWithMoreThan22DecimalPlaces_PreservesMantissa()
    {
        var info = BigValueInfo.ExponentFormatToBigValueInfo("0.00000000000000000000001e8");
        Assert.That(info.Mantissa, Is.EqualTo(1e-23));
        Assert.That(info.Exponent, Is.EqualTo(8));
    }

    [TestCase("")]
    [TestCase(null)]
    [TestCase("e5")]
    [TestCase("1e")]
    [TestCase("1.2.3")]
    [TestCase("1e5e3")]
    [TestCase("1e2.5")]
    [TestCase("1-2e5")]
    [TestCase("12x")]
    [TestCase("1e92233720368547758079")]
    [TestCase("1e9223372036854775808")]
    [TestCase("1e-9223372036854775809")]
    public void Parse_InvalidInput_ThrowsFormatException(string? input)
    {
        Assert.Throws<FormatException>(() => BigValueInfo.ExponentFormatToBigValueInfo(input!));
    }
}
