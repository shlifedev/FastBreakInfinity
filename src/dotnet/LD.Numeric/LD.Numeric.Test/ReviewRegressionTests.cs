using System.Globalization;
using LD.Numeric.IdleNumber;

namespace LD.Numeric.Test;

public class ReviewRegressionTests
{
    [Test]
    public void ToString_FractionalValues()
    {
        Assert.That(new BigDouble(0.5).ToString(), Is.EqualTo("0.5"));
        Assert.That(new BigDouble(0.05).ToString(), Is.EqualTo("0.05"));
        Assert.That(new BigDouble(0.001).ToString(), Is.EqualTo("0.001"));
        Assert.That(new BigDouble(0.25).ToString(), Is.EqualTo("0.25"));
        Assert.That(new BigDouble(-0.5).ToString(), Is.EqualTo("-0.5"));
    }

    [Test]
    public void ToFloat_UsesExponent()
    {
        Assert.That(new BigDouble(5e10).ToFloat(), Is.EqualTo(5e10f));
        Assert.That(new BigDouble(1234).ToFloat(), Is.EqualTo(1234f));
        Assert.That(new BigDouble(-2.5e5).ToFloat(), Is.EqualTo(-2.5e5f));
        Assert.That(new BigDouble(1, 100).ToFloat(), Is.EqualTo(float.PositiveInfinity));
    }

    [Test]
    public void Pow_NegativeTen_KeepsSign()
    {
        Assert.That(BigDouble.Pow(new BigDouble(-10), 3L) == new BigDouble(-1000), Is.True);
        Assert.That(BigDouble.Pow(new BigDouble(-10), 2L) == new BigDouble(100), Is.True);
        Assert.That(BigDouble.Pow(new BigDouble(-10), 3.0) == new BigDouble(-1000), Is.True);
    }

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

    [Test]
    public void PowersOf10_MatchCorrectlyRoundedValues()
    {
        for (long e = -323; e <= 308; e++)
        {
            double expected = double.Parse("1e" + e, CultureInfo.InvariantCulture);
            Assert.That(new BigDouble(1, e).ToDouble(), Is.EqualTo(expected), $"1e{e}");
        }
    }

    [Test]
    public void AlphabetConverter_PowerOfTenBoundary()
    {
        Assert.That(AlphabetConverter.GetExponent(1e21), Is.EqualTo(21));
        Assert.That(1e21.ConvertToAlphabetUnit(), Is.EqualTo("1.00G"));
    }

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
        Assert.That(FastDouble.ParseDouble("1e8589934592"), Is.EqualTo(double.PositiveInfinity));
    }

    [Test]
    public void GetExponentFromAlphabetUnit_LongUnit_NoOverflow()
    {
        var range = BigDouble.GetExponentFromAlphabetUnit("HAAAAAA");
        Assert.That(range.rangeA, Is.EqualTo(7451048517L));
        Assert.That(range.rangeB, Is.EqualTo(7451048519L));

        Assert.That(BigDouble.GetExponentFromAlphabetUnit("A"), Is.EqualTo((3L, 5L)));
        Assert.That(BigDouble.GetExponentFromAlphabetUnit("B"), Is.EqualTo((6L, 8L)));
    }
}
