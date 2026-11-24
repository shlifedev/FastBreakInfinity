using LD.Numeric.IdleNumber;

namespace LD.Numeric.Test;

/// <summary>
/// BigDouble Equals 메서드 테스트
/// </summary>
public class EqualsTests
{
    [Test]
    public void Equals_ExactMatch()
    {
        BigDouble a = new BigDouble("1.23456789e100");
        BigDouble b = new BigDouble("1.23456789e100");

        Assert.That(a.Equals(b), Is.True);
        Assert.That(a == b, Is.True);
    }

    [Test]
    public void Equals_WithTolerance()
    {
        BigDouble a = new BigDouble(1.0, 100);
        BigDouble b = new BigDouble(1.0 + 1e-10, 100);

        Assert.That(a.Equals(b), Is.False, "정확한 비교는 false");
        Assert.That(a.Equals(b, 1e-9), Is.True, "1e-9 tolerance로는 같음");
        Assert.That(a.Equals(b, 1e-11), Is.False, "1e-11 tolerance로는 다름");
    }

    [Test]
    public void Equals_DifferentRepresentationSameValue()
    {
        BigDouble a = new BigDouble(10.0, 99);
        BigDouble b = new BigDouble(1.0, 100);

        Assert.That(a == b, Is.True, "10e99 == 1e100");
    }

    [TestCase(double.NaN)]
    [TestCase(double.PositiveInfinity)]
    [TestCase(double.NegativeInfinity)]
    public void EqualityContract_SpecialKeysCanBeRetrieved(double mantissa)
    {
        var key = new BigDouble(mantissa);
        var copy = BigDouble.FromMantissaExponentNoNormalize(mantissa, 17);
        var table = new Dictionary<BigDouble, string> { [key] = "value" };
        Assert.That(copy.Equals(key), Is.True);
        Assert.That(copy.GetHashCode(), Is.EqualTo(key.GetHashCode()));
        Assert.That(table.TryGetValue(copy, out var found), Is.True);
        Assert.That(found, Is.EqualTo("value"));
        Assert.That(copy == key, Is.EqualTo(!double.IsNaN(mantissa)));
    }

    [Test]
    public void EqualityContract_ToleranceSeparatesInfinityAndFiniteValues()
    {
        Assert.That(BigDouble.One.Equals(BigDouble.PositiveInfinity, 1e-6), Is.False);
        Assert.That(BigDouble.NegativeInfinity.Equals(BigDouble.PositiveInfinity, 1e-6), Is.False);
        Assert.That(BigDouble.NegativeInfinity.Equals(BigDouble.NegativeInfinity, 1e-6), Is.True);
        Assert.That(BigDouble.NaN.Equals(BigDouble.NaN, 1e-6), Is.False);
    }

    [Test]
    public void EqualityContract_ToleranceRetainsAdjacentValues()
    {
        var left = new BigDouble(1, long.MaxValue);
        var right = new BigDouble(Math.BitIncrement(1.0), long.MaxValue);
        Assert.That(left.Equals(right, 0), Is.False);
        Assert.That(left.Equals(right, 1e-17), Is.False);
        Assert.That(left.Equals(right, 1e-15), Is.True);
        var tiny = BigDouble.Pow10(long.MinValue);
        Assert.That(left.Equals(tiny, 0.9), Is.False);
        Assert.That(left.Equals(tiny, 1), Is.True);
        Assert.That(left.Equals(-tiny, 1), Is.False);
    }
}
