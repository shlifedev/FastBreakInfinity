using LD.Numeric.IdleNumber;

namespace LD.Numeric.Test;

/// <summary>
/// FastDouble 종합 테스트 — ParseDouble과 OptimizeToString
/// </summary>
public class FastDoubleTests
{
    // ===== ParseDouble =====

    [Test]
    public void ParseDouble_PlainInteger()
    {
        Assert.That(FastDouble.ParseDouble("123"), Is.EqualTo(123.0));
        Assert.That(FastDouble.ParseDouble("0"), Is.EqualTo(0.0));
    }

    [Test]
    public void ParseDouble_Signs()
    {
        Assert.That(FastDouble.ParseDouble("+5"), Is.EqualTo(5.0));
        Assert.That(FastDouble.ParseDouble("-3.5"), Is.EqualTo(-3.5));
        Assert.That(FastDouble.ParseDouble("-0"), Is.EqualTo(0.0));
    }

    [Test]
    public void ParseDouble_NullOrEmpty_ReturnsZero()
    {
        Assert.That(FastDouble.ParseDouble(""), Is.EqualTo(0.0));
        Assert.That(FastDouble.ParseDouble(null!), Is.EqualTo(0.0));
    }

    [Test]
    public void ParseDouble_DecimalWithinAccuracy()
    {
        Assert.That(FastDouble.ParseDouble("123.456789"), Is.EqualTo(123.456789));
    }

    [Test]
    public void ParseDouble_TruncatesBeyondMaxDecimalPlaces()
    {
        // 반올림이 아니라 잘라냄
        Assert.That(FastDouble.ParseDouble("123.456789", 2), Is.EqualTo(123.45));
        Assert.That(FastDouble.ParseDouble("1.999999999", 3), Is.EqualTo(1.999));
    }

    [Test]
    public void ParseDouble_NegativeMaxDecimalPlaces_TreatedAsZero()
    {
        Assert.That(FastDouble.ParseDouble("123.456", -1), Is.EqualTo(123.0));
    }

    [Test]
    public void ParseDouble_ExponentNotation()
    {
        Assert.That(FastDouble.ParseDouble("1e5"), Is.EqualTo(100000.0));
        Assert.That(FastDouble.ParseDouble("1.5e3"), Is.EqualTo(1500.0));
        Assert.That(FastDouble.ParseDouble("1e+5"), Is.EqualTo(100000.0));
        Assert.That(FastDouble.ParseDouble("2e-3"), Is.EqualTo(0.002).Within(1e-18));
    }

    [TestCase("12a34")]
    [TestCase("1.2.3")]
    [TestCase("5e3e2")]
    [TestCase("--5")]
    [TestCase("1,000")]
    public void ParseDouble_InvalidInput_ThrowsFormatException(string input)
    {
        Assert.Throws<FormatException>(() => FastDouble.ParseDouble(input));
    }

    [Test]
    public void ParseDouble_RoundTripsWithBigDoubleValues()
    {
        // 게임 세이브 데이터 라운드트립 시나리오
        double[] values = { 0, 1, 999.5, 12345.678901, 1e15, 5e-5 };
        foreach (var value in values)
        {
            var str = value.ToString("R", System.Globalization.CultureInfo.InvariantCulture);
            var parsed = FastDouble.ParseDouble(str, 17);
            Assert.That(
                parsed,
                Is.EqualTo(value).Within(Math.Abs(value) * 1e-12 + 1e-12),
                $"'{str}' 라운드트립 실패"
            );
        }
    }

    // ===== OptimizeToString =====

    [Test]
    public void OptimizeToString_BasicRounding()
    {
        Assert.That((1.567).OptimizeToString(2), Is.EqualTo("1.57"));
        Assert.That((123.456).OptimizeToString(1), Is.EqualTo("123.5"));
        Assert.That((1.234).OptimizeToString(3), Is.EqualTo("1.234"));
    }

    [Test]
    public void OptimizeToString_CarryPropagation()
    {
        Assert.That((0.999999999).OptimizeToString(2), Is.EqualTo("1.00"));
        Assert.That((1.999).OptimizeToString(2), Is.EqualTo("2.00"));
        Assert.That((-1.999).OptimizeToString(2), Is.EqualTo("-2.00"));
    }

    [Test]
    public void OptimizeToString_NegativeValues()
    {
        Assert.That((-1.25).OptimizeToString(2), Is.EqualTo("-1.25"));
        Assert.That((-0.5).OptimizeToString(2), Is.EqualTo("-0.50"));
    }

    [Test]
    public void OptimizeToString_Zero_PadsDecimals()
    {
        Assert.That((0.0).OptimizeToString(3), Is.EqualTo("0.000"));
        Assert.That((0.0).OptimizeToString(1), Is.EqualTo("0.0"));
    }

    [Test]
    public void OptimizeToString_ZeroDecimalPlaces()
    {
        Assert.That((1.9).OptimizeToString(0), Is.EqualTo("2"));
        Assert.That((42.0).OptimizeToString(0), Is.EqualTo("42"));
    }

    [Test]
    public void OptimizeToString_SpecialValues()
    {
        Assert.That(double.NaN.OptimizeToString(2), Is.EqualTo("NaN"));
        Assert.That(double.PositiveInfinity.OptimizeToString(2), Is.EqualTo("Infinity"));
        Assert.That(double.NegativeInfinity.OptimizeToString(2), Is.EqualTo("-Infinity"));
    }

    [Test]
    public void OptimizeToString_HugeValue_FallsBackToStandardFormatter()
    {
        Assert.That((1e19).OptimizeToString(2), Is.EqualTo("10000000000000000000.00"));
    }

    [Test]
    public void OptimizeToString_NegativeDecimalPlaces_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => (1.5).OptimizeToString(-1));
    }

    [Test]
    public void OptimizeToString_SmallFraction_PadsLeadingZeros()
    {
        // 소수부 앞자리 0이 유지돼야 함 (1.05 → "1.05", "1.5" 아님)
        Assert.That((1.05).OptimizeToString(2), Is.EqualTo("1.05"));
        Assert.That((10.001).OptimizeToString(3), Is.EqualTo("10.001"));
    }
}
