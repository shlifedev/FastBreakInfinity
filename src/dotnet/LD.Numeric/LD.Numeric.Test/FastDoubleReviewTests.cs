using System.Globalization;
using LD.Numeric.IdleNumber;

namespace LD.Numeric.Test;

public class FastDoubleReviewTests
{
    [Test]
    public void ParseDouble_ExponentNotation_KeepsFullPrecision()
    {
        Assert.That(FastDouble.ParseDouble("0.0000001234567e10"), Is.EqualTo(1234.567));
        Assert.That(FastDouble.ParseDouble("1.23456789e10"), Is.EqualTo(12345678900.0));
        Assert.That(FastDouble.ParseDouble("1.23456789e-2"), Is.EqualTo(0.0123456789));
    }

    [Test]
    public void ParseDouble_PlainNotation_StillTruncates()
    {
        Assert.That(FastDouble.ParseDouble("1.9999999", 6), Is.EqualTo(1.999999));
        Assert.That(FastDouble.ParseDouble("0.0000001", 6), Is.EqualTo(0));
    }

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
        Assert.That(FastDouble.ParseDouble(""), Is.EqualTo(0));
        Assert.That(FastDouble.ParseDouble((string)null!), Is.EqualTo(0));
    }

    [Test]
    public void OptimizeToString_ZeroDecimalPlaces_UsesFastPath()
    {
        Assert.That(1234.567.OptimizeToString(0), Is.EqualTo("1235"));
        Assert.That((-1234.6).OptimizeToString(0), Is.EqualTo("-1235"));
        Assert.That(0.0.OptimizeToString(0), Is.EqualTo("0"));
        Assert.That(double.PositiveInfinity.OptimizeToString(0), Is.EqualTo("Infinity"));
        Assert.That(double.NaN.OptimizeToString(0), Is.EqualTo("NaN"));
    }

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

    [TestCase(0.5000152587890625, 17, "0.50001525878906250")]
    [TestCase(-2.5000152587890625, 17, "-2.50001525878906250")]
    [TestCase(0.49591827392578125, 16, "0.4959182739257813")]
    [TestCase(-0.500003814697265625, 17, "-0.50000381469726563")]
    [TestCase(0.000000000000000009, 17, "0.00000000000000001")]
    [TestCase(double.Epsilon, 17, "0.00000000000000000")]
    public void OptimizeToString_HighPrecision_RoundsTheStoredFraction(
        double value,
        int places,
        string expected
    )
    {
        Assert.That(value.OptimizeToString(places), Is.EqualTo(expected));
    }

    [TestCase(0, "-0")]
    [TestCase(2, "-0.00")]
    public void OptimizeToString_NegativeZero_PreservesSign(int places, string expected)
    {
        Assert.That((-0.0).OptimizeToString(places), Is.EqualTo(expected));
    }

    [Test]
    public void ParseDouble_SpanOverload()
    {
        Assert.That(FastDouble.ParseDouble("123.45".AsSpan()), Is.EqualTo(123.45));
        Assert.That(FastDouble.ParseDouble("price:1.5e3".AsSpan(6)), Is.EqualTo(1500));
    }
}
