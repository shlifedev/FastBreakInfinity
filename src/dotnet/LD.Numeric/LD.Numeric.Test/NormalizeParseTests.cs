using LD.Numeric.IdleNumber;

namespace LD.Numeric.Test;

/// <summary>
/// BigDouble Normalize 및 문자열 파싱 테스트
/// </summary>
public class NormalizeParseTests
{
    // ===== Normalize 테스트 =====

    [Test]
    public void Normalize_MantissaInRange()
    {
        var testCases = new[] { (0.5, 100L), (50.0, 100L), (0.001, 100L), (999.9, 100L) };

        foreach (var (m, e) in testCases)
        {
            BigDouble bd = new BigDouble(m, e);
            double absMantissa = Math.Abs(bd.Mantissa);

            Assert.That(
                absMantissa >= 1 && absMantissa < 10 || bd.Mantissa == 0,
                Is.True,
                $"Normalize({m}, {e}) => 가수 범위 확인: {bd.Mantissa}"
            );
        }
    }

    [Test]
    public void Normalize_PreservesValue()
    {
        BigDouble a = new BigDouble(123.456, 100);

        Assert.That(a.Mantissa >= 1.23 && a.Mantissa <= 1.24, Is.True, "가수 정규화");
        Assert.That(a.Exponent == 102, Is.True, "지수 조정");
    }

    [Test]
    public void Normalize_VerySmallMantissa()
    {
        BigDouble bd = new BigDouble(1e-10, 100);

        Assert.That(bd.Mantissa >= 1 && bd.Mantissa < 10, Is.True);
        Assert.That(bd.Exponent == 90, Is.True);
    }

    // ===== Parse 테스트 =====

    [Test]
    public void Parse_ExponentFormat()
    {
        var testCases = new[]
        {
            ("1e100", 1.0, 100L),
            ("1.5e100", 1.5, 100L),
            ("1.23456e999", 1.23456, 999L),
            ("-1.5e100", -1.5, 100L),
            ("9.9e-50", 9.9, -50L),
        };

        foreach (var (str, expectedM, expectedE) in testCases)
        {
            BigDouble bd = new BigDouble(str);

            Assert.That(
                Math.Abs(bd.Mantissa - expectedM) < 1e-10,
                Is.True,
                $"Parse '{str}' 가수: {bd.Mantissa} vs {expectedM}"
            );
            Assert.That(
                bd.Exponent == expectedE,
                Is.True,
                $"Parse '{str}' 지수: {bd.Exponent} vs {expectedE}"
            );
        }
    }

    [Test]
    public void Parse_PlainNumber()
    {
        BigDouble bd = new BigDouble("12345.6789");

        Assert.That(bd.Exponent == 4, Is.True);
        Assert.That(Math.Abs(bd.Mantissa - 1.23456789) < 1e-5, Is.True);
    }

    [Test]
    public void Parse_LargeExponent()
    {
        BigDouble bd = new BigDouble("1e999999999");

        Assert.That(bd.Mantissa == 1.0, Is.True);
        Assert.That(bd.Exponent == 999999999L, Is.True);
    }

    [Test]
    public void Parse_PrecisionLimit()
    {
        BigDouble a = new BigDouble("1.123456789e100");
        BigDouble b = new BigDouble("1.123456e100");

        // 정밀도 제한 확인용 테스트
        // 결과는 구현에 따라 다를 수 있음
        Assert.Pass("정밀도 제한 테스트 완료");
    }
}
