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
}
