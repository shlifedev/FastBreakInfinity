using LD.Numeric.IdleNumber;

namespace LD.Numeric.Test;

public class RoundingTests
{
    [Test]
    public void Round_DefaultIsBankersRounding()
    {
        Assert.That(BigDouble.Round(new BigDouble(2.5)) == new BigDouble(2), Is.True);
        Assert.That(BigDouble.Round(new BigDouble(3.5)) == new BigDouble(4), Is.True);
    }

    [Test]
    public void Round_MidpointRoundingAwayFromZero()
    {
        Assert.That(
            BigDouble.Round(new BigDouble(2.5), MidpointRounding.AwayFromZero) == new BigDouble(3),
            Is.True
        );
        Assert.That(
            BigDouble.Round(new BigDouble(-2.5), MidpointRounding.AwayFromZero)
                == new BigDouble(-3),
            Is.True
        );
    }

    [Test]
    public void Round_TinyValue_ReturnsZero()
    {
        Assert.That(BigDouble.Round(new BigDouble(0.04)) == BigDouble.Zero, Is.True);
        Assert.That(BigDouble.Round(new BigDouble(-0.04)) == BigDouble.Zero, Is.True);
    }

    [Test]
    public void Round_HugeValue_Unchanged()
    {
        var huge = new BigDouble(1.23456, 20);
        Assert.That(BigDouble.Round(huge) == huge, Is.True);
    }

    [Test]
    public void Round_NaN_PassesThrough()
    {
        Assert.That(BigDouble.IsNaN(BigDouble.Round(BigDouble.NaN)), Is.True);
        Assert.That(
            BigDouble.IsNaN(BigDouble.Round(BigDouble.NaN, MidpointRounding.AwayFromZero)),
            Is.True
        );
    }

    [Test]
    public void Floor_BasicValues()
    {
        Assert.That(BigDouble.Floor(new BigDouble(2.9)) == new BigDouble(2), Is.True);
        Assert.That(BigDouble.Floor(new BigDouble(-0.5)) == new BigDouble(-1), Is.True);
        Assert.That(BigDouble.Floor(new BigDouble(-2.1)) == new BigDouble(-3), Is.True);
    }

    [Test]
    public void Floor_TinyValue_BranchBySign()
    {
        Assert.That(BigDouble.Floor(new BigDouble(0.05)) == BigDouble.Zero, Is.True);
        Assert.That(BigDouble.Floor(new BigDouble(-0.05)) == -BigDouble.One, Is.True);
    }

    [Test]
    public void Floor_HugeValue_Unchanged()
    {
        var huge = new BigDouble(1.5, 50);
        Assert.That(BigDouble.Floor(huge) == huge, Is.True);
    }

    [Test]
    public void Ceiling_BasicValues()
    {
        Assert.That(BigDouble.Ceiling(new BigDouble(2.1)) == new BigDouble(3), Is.True);
        Assert.That(BigDouble.Ceiling(new BigDouble(-2.9)) == new BigDouble(-2), Is.True);
    }

    [Test]
    public void Ceiling_TinyValue_BranchBySign()
    {
        Assert.That(BigDouble.Ceiling(new BigDouble(0.05)) == BigDouble.One, Is.True);
        Assert.That(BigDouble.Ceiling(new BigDouble(-0.05)) == BigDouble.Zero, Is.True);
    }

    [Test]
    public void Ceiling_NaN_PassesThrough()
    {
        Assert.That(BigDouble.IsNaN(BigDouble.Ceiling(BigDouble.NaN)), Is.True);
    }

    [Test]
    public void Truncate_BasicValues()
    {
        Assert.That(BigDouble.Truncate(new BigDouble(5.7)) == new BigDouble(5), Is.True);
        Assert.That(BigDouble.Truncate(new BigDouble(-5.7)) == new BigDouble(-5), Is.True);
    }

    [Test]
    public void Truncate_FractionOnly_ReturnsZero()
    {
        Assert.That(BigDouble.Truncate(new BigDouble(0.9)) == BigDouble.Zero, Is.True);
        Assert.That(BigDouble.Truncate(new BigDouble(-0.9)) == BigDouble.Zero, Is.True);
    }

    [Test]
    public void Truncate_HugeValue_Unchanged()
    {
        var huge = new BigDouble(9.87654, 30);
        Assert.That(BigDouble.Truncate(huge) == huge, Is.True);
    }

    [Test]
    public void Truncate_NaN_PassesThrough()
    {
        Assert.That(BigDouble.IsNaN(BigDouble.Truncate(BigDouble.NaN)), Is.True);
    }

    [Test]
    public void Floor_PurchaseCountScenario()
    {
        BigDouble money = new BigDouble(9.99, 5);
        BigDouble price = new BigDouble(1, 5);
        var count = BigDouble.Floor(money / price);
        Assert.That(count == new BigDouble(9), Is.True, $"기대 9, 실제 {count}");
    }

    [TestCase(1, MidpointRounding.ToPositiveInfinity, 1)]
    [TestCase(-1, MidpointRounding.ToNegativeInfinity, -1)]
    [TestCase(1, MidpointRounding.ToZero, 0)]
    public void TinyDirectedRoundHonorsMode(int sign, MidpointRounding mode, int expected)
    {
        var number = new BigDouble(sign, -500);
        Assert.That(BigDouble.Round(number, mode), Is.EqualTo(new BigDouble(expected)));
    }
}
