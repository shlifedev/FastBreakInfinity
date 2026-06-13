using LD.Numeric.IdleNumber;

namespace LD.Numeric.Test;

/// <summary>
/// BigDouble 경계값 및 극단 케이스 테스트
/// </summary>
public class EdgeCaseTests
{
    // ===== AdjustedMantissa 테스트 =====

    [Test]
    public void AdjustedMantissa_DoesNotMutateOriginalValue()
    {
        BigDouble original = new BigDouble(1.23456789, 100);
        double originalMantissa = original.Mantissa;

        double adjusted = original.AdjustedMantissa();

        Assert.That(original.Mantissa, Is.EqualTo(originalMantissa));
        Assert.That(adjusted, Is.EqualTo(12.34568).Within(1e-12));
    }

    [Test]
    public void AdjustedMantissa_ConsistentResults()
    {
        BigDouble bd = new BigDouble(1.234567, 5);

        double first = bd.AdjustedMantissa();
        double second = bd.AdjustedMantissa();

        Assert.That(first == second, Is.True, "연속 호출 시 일관된 결과");
    }

    // ===== 경계값 테스트 =====

    [Test]
    public void EdgeCase_MaxExponent()
    {
        BigDouble bd = new BigDouble(1.0, long.MaxValue - 1);

        Assert.That(bd.Exponent, Is.EqualTo(long.MaxValue - 1));
    }

    [Test]
    public void EdgeCase_MinExponent()
    {
        BigDouble bd = new BigDouble(1.0, long.MinValue + 1);

        Assert.That(BigDouble.IsNaN(bd), Is.False);
        Assert.That(bd.Exponent, Is.EqualTo(long.MinValue + 1));
    }

    [Test]
    public void EdgeCase_DoublePrecisionLimit()
    {
        double one = 1.0;
        double nextUp = BitConverter.Int64BitsToDouble(BitConverter.DoubleToInt64Bits(one) + 1);
        BigDouble a = new BigDouble(one, 100);
        BigDouble b = new BigDouble(nextUp, 100);

        Assert.That(a.Mantissa, Is.Not.EqualTo(b.Mantissa));
        Assert.That(a != b, Is.True);
        Assert.That(a < b, Is.True);
    }

    [Test]
    public void EdgeCase_Denormalized()
    {
        BigDouble bd = new BigDouble(double.Epsilon, 0);

        Assert.That(BigDouble.IsNaN(bd), Is.False);
        Assert.That(BigDouble.IsInfinity(bd), Is.False);
        Assert.That(bd.Mantissa, Is.EqualTo(5.0));
        Assert.That(bd.Exponent, Is.EqualTo(-324));
        Assert.That(bd.ToDouble(), Is.EqualTo(double.Epsilon));
    }
}
