using LD.Numeric.IdleNumber;

namespace LD.Numeric.Test;

/// <summary>
/// NumberUtility.GetDigits 테스트 — 정수부 자릿수 계산
/// </summary>
public class NumberUtilityTests
{
    [TestCase(0.0, 0)]
    [TestCase(5.0, 1)]
    [TestCase(9.0, 1)]
    [TestCase(10.0, 2)]
    [TestCase(99.0, 2)]
    [TestCase(100.0, 3)]
    [TestCase(999.99, 3)]
    [TestCase(1000.0, 4)]
    public void GetDigits_PositiveValues(double value, int expected)
    {
        Assert.That(NumberUtility.GetDigits(value), Is.EqualTo(expected));
    }

    [TestCase(-5.0, 1)]
    [TestCase(-99.0, 2)]
    [TestCase(-1234.0, 4)]
    public void GetDigits_NegativeValues_UsesAbsoluteValue(double value, int expected)
    {
        Assert.That(NumberUtility.GetDigits(value), Is.EqualTo(expected));
    }

    [TestCase(0.5)]
    [TestCase(0.999)]
    [TestCase(-0.001)]
    public void GetDigits_FractionLessThanOne_ReturnsZero(double value)
    {
        Assert.That(NumberUtility.GetDigits(value), Is.EqualTo(0));
    }

    [Test]
    public void GetDigits_NaN_ReturnsZero()
    {
        Assert.That(NumberUtility.GetDigits(double.NaN), Is.EqualTo(0));
    }

    [Test]
    public void GetDigits_Infinity_ReturnsZero()
    {
        Assert.That(NumberUtility.GetDigits(double.PositiveInfinity), Is.EqualTo(0));
        Assert.That(NumberUtility.GetDigits(double.NegativeInfinity), Is.EqualTo(0));
    }

    [Test]
    public void GetDigits_IntMaxValue()
    {
        Assert.That(NumberUtility.GetDigits(int.MaxValue), Is.EqualTo(10));
    }
}
