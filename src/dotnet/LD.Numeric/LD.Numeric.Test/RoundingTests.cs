using LD.Numeric.IdleNumber;

namespace LD.Numeric.Test;

/// <summary>
/// BigDouble 반올림 계열 함수의 분기별 테스트 — Round/Floor/Ceiling/Truncate
/// </summary>
public class RoundingTests
{
    // ===== Round =====

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
        // 지수 < -1이면 0
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

    // ===== Floor =====

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
        // 지수 < -1: 양수는 0, 음수는 -1
        Assert.That(BigDouble.Floor(new BigDouble(0.05)) == BigDouble.Zero, Is.True);
        Assert.That(BigDouble.Floor(new BigDouble(-0.05)) == -BigDouble.One, Is.True);
    }

    [Test]
    public void Floor_HugeValue_Unchanged()
    {
        var huge = new BigDouble(1.5, 50);
        Assert.That(BigDouble.Floor(huge) == huge, Is.True);
    }

    // ===== Ceiling =====

    [Test]
    public void Ceiling_BasicValues()
    {
        Assert.That(BigDouble.Ceiling(new BigDouble(2.1)) == new BigDouble(3), Is.True);
        Assert.That(BigDouble.Ceiling(new BigDouble(-2.9)) == new BigDouble(-2), Is.True);
    }

    [Test]
    public void Ceiling_TinyValue_BranchBySign()
    {
        // 지수 < -1: 양수는 1, 음수는 0
        Assert.That(BigDouble.Ceiling(new BigDouble(0.05)) == BigDouble.One, Is.True);
        Assert.That(BigDouble.Ceiling(new BigDouble(-0.05)) == BigDouble.Zero, Is.True);
    }

    [Test]
    public void Ceiling_NaN_PassesThrough()
    {
        Assert.That(BigDouble.IsNaN(BigDouble.Ceiling(BigDouble.NaN)), Is.True);
    }

    // ===== Truncate =====

    [Test]
    public void Truncate_BasicValues()
    {
        Assert.That(BigDouble.Truncate(new BigDouble(5.7)) == new BigDouble(5), Is.True);
        Assert.That(BigDouble.Truncate(new BigDouble(-5.7)) == new BigDouble(-5), Is.True);
    }

    [Test]
    public void Truncate_FractionOnly_ReturnsZero()
    {
        // 지수 < 0이면 0 (Floor와 달리 음수도 0으로)
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

    // ===== 게임 시나리오: 구매 개수 내림 =====

    [Test]
    public void Floor_PurchaseCountScenario()
    {
        // 보유 재화 / 가격 → 구매 가능 개수 내림
        BigDouble money = new BigDouble(9.99, 5); // 999,000
        BigDouble price = new BigDouble(1, 5); // 100,000
        var count = BigDouble.Floor(money / price);
        Assert.That(count == new BigDouble(9), Is.True, $"기대 9, 실제 {count}");
    }
}
