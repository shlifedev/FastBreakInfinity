using LD.Numeric.IdleNumber;

namespace LD.Numeric.Test;

/// <summary>
/// BigDouble 비교 연산자 테스트
/// </summary>
public class ComparisonTests
{
    [Test]
    public void Comparison_SameExponent_DifferentMantissa()
    {
        // 같은 지수, 다른 가수 비교
        BigDouble a = new BigDouble(1.1, 100);
        BigDouble b = new BigDouble(1.2, 100);

        Assert.That(a < b, Is.True, "1.1e100 < 1.2e100");
        Assert.That(b > a, Is.True, "1.2e100 > 1.1e100");
        Assert.That(a != b, Is.True, "1.1e100 != 1.2e100");
    }

    [Test]
    public void Comparison_SmallMantissaDifference()
    {
        // 소수점 아래 미세한 차이 비교
        BigDouble a = new BigDouble(1.000000001, 100);
        BigDouble b = new BigDouble(1.000000002, 100);

        Assert.That(a < b, Is.True, "1.000000001e100 < 1.000000002e100");
        Assert.That(a != b, Is.True, "미세 차이도 구분해야 함");
    }

    [Test]
    public void Comparison_VerySmallMantissaDifference()
    {
        // 더 미세한 차이 (Tolerance = 1e-18 근처)
        BigDouble a = new BigDouble(1.0, 100);
        BigDouble b = new BigDouble(1.0 + 1e-15, 100);

        // double 정밀도 한계 내에서 다르다면 다르게 인식해야 함
        if (Math.Abs(b.Mantissa - a.Mantissa) > double.Epsilon)
        {
            Assert.That(a != b, Is.True, "감지 가능한 차이는 다르게 처리해야 함");
        }
    }

    [Test]
    public void Comparison_DifferentExponent()
    {
        // 다른 지수 비교 - 지수가 크면 무조건 큼
        BigDouble small = new BigDouble(9.9, 99);
        BigDouble large = new BigDouble(1.1, 100);

        Assert.That(small < large, Is.True, "9.9e99 < 1.1e100");
        Assert.That(large > small, Is.True, "1.1e100 > 9.9e99");
    }

    [Test]
    public void Comparison_AdjacentExponent()
    {
        // 인접 지수 비교 (경계값)
        BigDouble a = new BigDouble(9.999999999999999, 99);
        BigDouble b = new BigDouble(1.0, 100);

        Assert.That(a < b, Is.True, "9.999...e99 < 1.0e100");
    }

    [Test]
    public void Comparison_NegativeNumbers()
    {
        // 음수 비교
        BigDouble a = new BigDouble(-1.1, 100);
        BigDouble b = new BigDouble(-1.2, 100);

        Assert.That(a > b, Is.True, "-1.1e100 > -1.2e100");
        Assert.That(b < a, Is.True, "-1.2e100 < -1.1e100");
    }

    [Test]
    public void Comparison_NegativeVsPositive()
    {
        BigDouble positive = new BigDouble(1.0, 100);
        BigDouble negative = new BigDouble(-1.0, 100);

        Assert.That(positive > negative, Is.True, "양수 > 음수");
        Assert.That(negative < positive, Is.True, "음수 < 양수");
        Assert.That(positive > BigDouble.Zero, Is.True, "양수 > 0");
        Assert.That(negative < BigDouble.Zero, Is.True, "음수 < 0");
    }

    [Test]
    public void Comparison_NegativeDifferentExponent()
    {
        BigDouble a = new BigDouble(-1.1, 100);
        BigDouble b = new BigDouble(-1.1, 99);

        Assert.That(a < b, Is.True, "-1.1e100 < -1.1e99");
    }

    [Test]
    public void Comparison_SpecialValues_NaN()
    {
        BigDouble nan = BigDouble.NaN;
        BigDouble sameNan = BigDouble.NaN;
        BigDouble normal = new BigDouble(1.0, 100);

        Assert.That(nan == sameNan, Is.False, "NaN == NaN should be false");
        Assert.That(nan < normal, Is.False, "NaN < normal should be false");
        Assert.That(nan > normal, Is.False, "NaN > normal should be false");
        Assert.That(nan <= normal, Is.False, "NaN <= normal should be false");
        Assert.That(nan >= normal, Is.False, "NaN >= normal should be false");
    }

    [Test]
    public void Comparison_SpecialValues_Infinity()
    {
        BigDouble posInf = BigDouble.PositiveInfinity;
        BigDouble negInf = BigDouble.NegativeInfinity;
        BigDouble normal = new BigDouble(9.9, 999999);

        Assert.That(posInf > normal, Is.True, "+Inf > any normal");
        Assert.That(negInf < normal, Is.True, "-Inf < any normal");
        Assert.That(posInf > negInf, Is.True, "+Inf > -Inf");
    }

    [Test]
    public void Comparison_SpecialValues_Zero()
    {
        BigDouble zero1 = BigDouble.Zero;
        BigDouble zero2 = new BigDouble(0.0);
        BigDouble positive = new BigDouble(1e-300);
        BigDouble negative = new BigDouble(-1e-300);

        Assert.That(zero1 == zero2, Is.True, "Zero == Zero");
        Assert.That(zero1 < positive, Is.True, "0 < 양수");
        Assert.That(zero1 > negative, Is.True, "0 > 음수");
    }
}
