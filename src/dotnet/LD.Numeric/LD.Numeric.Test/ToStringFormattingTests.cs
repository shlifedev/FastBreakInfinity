using LD.Numeric.IdleNumber;

namespace LD.Numeric.Test;

public class ToStringFormattingTests
{
    [TestCase(999.49, "999")]
    [TestCase(999.4999998, "999")]
    [TestCase(-999.4999998, "-999")]
    [TestCase(999.5, "1.00A")]
    [TestCase(-999.5, "-1.00A")]
    [TestCase(999499.96, "999A")]
    [TestCase(999499.9998, "999A")]
    [TestCase(-999499.9998, "-999A")]
    [TestCase(999500, "1.00B")]
    [TestCase(-999500, "-1.00B")]
    [TestCase(9996, "10.0A")]
    [TestCase(99960, "100A")]
    public void ToString_RoundingUsesFinalUnitAndDigitCount(double value, string expected)
    {
        Assert.That(new BigDouble(value).ToString(), Is.EqualTo(expected));
    }

    [Test]
    public void ToString_AdjacentMantissas_StraddleUnitCarry()
    {
        const double midpoint = 9.995;
        Assert.That(new BigDouble(Math.BitDecrement(midpoint), 5).ToString(), Is.EqualTo("999A"));
        Assert.That(new BigDouble(midpoint, 5).ToString(), Is.EqualTo("1.00B"));
        Assert.That(new BigDouble(Math.BitIncrement(midpoint), 5).ToString(), Is.EqualTo("1.00B"));
    }

    [Test]
    public void ToString_CarryIntoLargestUnit_KeepsUnit()
    {
        var value = new BigDouble(9.999999, long.MaxValue - 2);
        Assert.That(
            value.ToString(),
            Is.EqualTo("1.00" + BigDouble.GetAlphabetUnit(long.MaxValue))
        );
    }

    [TestCase(double.PositiveInfinity, "Infinity")]
    [TestCase(double.NegativeInfinity, "-Infinity")]
    [TestCase(double.NaN, "NaN")]
    public void ToString_NonFinite_UsesInvariantSymbol(double value, string expected)
    {
        Assert.That(new BigDouble(value).ToString(), Is.EqualTo(expected));
    }

    [TestCase(2.5, "2.5e-400")]
    [TestCase(-2.5, "-2.5e-400")]
    public void ToString_FractionBelowDoubleRange_PreservesValue(double mantissa, string expected)
    {
        Assert.That(new BigDouble(mantissa, -400).ToString(), Is.EqualTo(expected));
    }

    [Test]
    public void ToString_Zero()
    {
        Assert.That(BigDouble.Zero.ToString(), Is.EqualTo("0"));
    }

    [Test]
    public void ToString_SmallIntegers_NoDecimals()
    {
        Assert.That(new BigDouble(7).ToString(), Is.EqualTo("7"));
        Assert.That(new BigDouble(12).ToString(), Is.EqualTo("12"));
        Assert.That(new BigDouble(999).ToString(), Is.EqualTo("999"));
    }

    [Test]
    public void ToString_ThousandsRange_TwoDecimalsWithUnit()
    {
        Assert.That(new BigDouble(1234).ToString(), Is.EqualTo("1.23A"));
        Assert.That(new BigDouble(5000).ToString(), Is.EqualTo("5.00A"));
    }

    [Test]
    public void ToString_TenThousandsRange_OneDecimalWithUnit()
    {
        Assert.That(new BigDouble(12345).ToString(), Is.EqualTo("12.3A"));
    }

    [Test]
    public void ToString_HundredThousandsRange_NoDecimalsWithUnit()
    {
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
        var expected = AlphabetManager.GetAlphabetUnit(1000 / 3 - 1);
        var str = new BigDouble(1.5, 1000).ToString();
        Assert.That(str, Does.EndWith(expected));
        Assert.That(str, Does.StartWith("1"));
    }

    [Test]
    public void ToString_MantissaRoundsToThreeDecimals()
    {
        Assert.That(new BigDouble(1.2341234, 3).ToString(), Is.EqualTo("1.23A"));
        Assert.That(new BigDouble(1.9991234, 3).ToString(), Is.EqualTo("2.00A"));
    }
}
