using LD.Numeric.IdleNumber;

namespace LD.Numeric.Test;

public class BigMathTests
{
    [Test]
    public void AffordGeometricSeries_Basic()
    {
        var affordable = BigMath.AffordGeometricSeries(100, 1, 2, 0);
        Assert.That(
            affordable.Equals(new BigDouble(6), 1e-9),
            Is.True,
            $"기대 6, 실제 {affordable}"
        );
    }

    [Test]
    public void AffordGeometricSeries_WithCurrentOwned()
    {
        var affordable = BigMath.AffordGeometricSeries(100, 1, 2, 2);
        Assert.That(
            affordable.Equals(new BigDouble(4), 1e-9),
            Is.True,
            $"기대 4, 실제 {affordable}"
        );
    }

    [Test]
    public void AffordGeometricSeries_CannotAffordAny()
    {
        var affordable = BigMath.AffordGeometricSeries(0.5, 1, 2, 0);
        Assert.That(
            affordable.Equals(BigDouble.Zero, 1e-9) || affordable == BigDouble.Zero,
            Is.True,
            $"기대 0, 실제 {affordable}"
        );
    }

    [Test]
    public void AffordGeometricSeries_HugeNumbers()
    {
        var affordable = BigMath.AffordGeometricSeries(
            new BigDouble(1, 100),
            new BigDouble(1, 10),
            10,
            0
        );
        Assert.That(
            affordable.Equals(new BigDouble(90), 1e-9),
            Is.True,
            $"기대 90, 실제 {affordable}"
        );
    }

    [Test]
    public void SumGeometricSeries_Basic()
    {
        var cost = BigMath.SumGeometricSeries(3, 10, 2, 0);
        Assert.That(cost.Equals(new BigDouble(70), 1e-9), Is.True, $"기대 70, 실제 {cost}");
    }

    [Test]
    public void SumGeometricSeries_WithCurrentOwned()
    {
        var cost = BigMath.SumGeometricSeries(2, 10, 2, 1);
        Assert.That(cost.Equals(new BigDouble(60), 1e-9), Is.True, $"기대 60, 실제 {cost}");
    }

    [Test]
    public void SumGeometricSeries_RatioOne_IsLinearCost()
    {
        var cost = BigMath.SumGeometricSeries(5, 10, 1, 0);
        Assert.That(cost.Equals(new BigDouble(50), 1e-9), Is.True, $"기대 50, 실제 {cost}");
    }

    [Test]
    public void AffordGeometricSeries_RatioOne_IsLinearAfford()
    {
        var affordable = BigMath.AffordGeometricSeries(100, 10, 1, 3);
        Assert.That(
            affordable.Equals(new BigDouble(10), 1e-9),
            Is.True,
            $"기대 10, 실제 {affordable}"
        );
    }

    [Test]
    public void SumAndAfford_GeometricSeries_AreConsistent()
    {
        BigDouble priceStart = 5;
        BigDouble ratio = 1.5;
        BigDouble owned = 3;
        BigDouble count = 7;

        var cost = BigMath.SumGeometricSeries(count, priceStart, ratio, owned);
        var affordable = BigMath.AffordGeometricSeries(cost, priceStart, ratio, owned);
        Assert.That(affordable.Equals(count, 1e-6), Is.True, $"기대 {count}, 실제 {affordable}");
    }

    [Test]
    public void AffordArithmeticSeries_Basic()
    {
        var affordable = BigMath.AffordArithmeticSeries(100, 10, 5, 0);
        Assert.That(
            affordable.Equals(new BigDouble(5), 1e-9),
            Is.True,
            $"기대 5, 실제 {affordable}"
        );
    }

    [Test]
    public void AffordArithmeticSeries_WithCurrentOwned()
    {
        var affordable = BigMath.AffordArithmeticSeries(50, 10, 5, 2);
        Assert.That(
            affordable.Equals(new BigDouble(2), 1e-9),
            Is.True,
            $"기대 2, 실제 {affordable}"
        );
    }

    [Test]
    public void SumArithmeticSeries_Basic()
    {
        var cost = BigMath.SumArithmeticSeries(4, 10, 5, 0);
        Assert.That(cost.Equals(new BigDouble(70), 1e-9), Is.True, $"기대 70, 실제 {cost}");
    }

    [Test]
    public void SumAndAfford_ArithmeticSeries_AreConsistent()
    {
        BigDouble priceStart = 100;
        BigDouble priceAdd = 25;
        BigDouble owned = 4;
        BigDouble count = 10;

        var cost = BigMath.SumArithmeticSeries(count, priceStart, priceAdd, owned);
        var affordable = BigMath.AffordArithmeticSeries(cost, priceStart, priceAdd, owned);
        Assert.That(affordable.Equals(count, 1e-6), Is.True, $"기대 {count}, 실제 {affordable}");
    }

    [Test]
    public void EfficiencyOfPurchase_Basic()
    {
        var efficiency = BigMath.EfficiencyOfPurchase(10, 100, 1);
        Assert.That(
            efficiency.Equals(new BigDouble(10.1), 1e-9),
            Is.True,
            $"기대 10.1, 실제 {efficiency}"
        );
    }

    [Test]
    public void EfficiencyOfPurchase_LowerIsBetter()
    {
        var better = BigMath.EfficiencyOfPurchase(100, 50, 20);
        var worse = BigMath.EfficiencyOfPurchase(100, 50, 5);
        Assert.That(better < worse, Is.True);
    }

    [Test]
    public void RandomBigDouble_AlwaysNormalized()
    {
        for (int i = 0; i < 1000; i++)
        {
            var value = BigMath.RandomBigDouble(100);
            Assert.That(BigDouble.IsNaN(value), Is.False);

            var m = Math.Abs(value.Mantissa);
            bool normalized = m == 0 || (m >= 1 && m < 10);
            Assert.That(normalized, Is.True, $"정규화 위반: {value.ToStringMantissaExponent()}");
        }
    }

    [TestCase(9, 0)]
    [TestCase(35, 3)]
    [TestCase(40, 4)]
    public void PurchaseCount_FlatArithmeticPrice(int resources, int expected)
    {
        Assert.That(
            BigMath.AffordArithmeticSeries(resources, 10, 0, 7),
            Is.EqualTo(new BigDouble(expected))
        );
    }

    [Test]
    public void PurchaseCount_LargeStartingPriceAndSmallIncrease()
    {
        var price = new BigDouble(1, 30);
        Assert.That(
            BigMath.AffordArithmeticSeries(price * 3.5, price, 1, 0),
            Is.EqualTo(new BigDouble(3))
        );
    }

    [Test]
    public void PurchaseCount_GeometricBudgetJustBelowTotal()
    {
        var cost = BigMath.SumGeometricSeries(6, 1, 2, 0);
        Assert.That(BigMath.AffordGeometricSeries(cost, 1, 2, 0), Is.EqualTo(new BigDouble(6)));
        Assert.That(
            BigMath.AffordGeometricSeries(cost - 1e-9, 1, 2, 0),
            Is.EqualTo(new BigDouble(5))
        );
    }

    [Test]
    public void PurchaseCount_ArithmeticBudgetJustBelowTotal()
    {
        var cost = BigMath.SumArithmeticSeries(5, 10, 5, 0);
        Assert.That(BigMath.AffordArithmeticSeries(cost, 10, 5, 0), Is.EqualTo(new BigDouble(5)));
        Assert.That(
            BigMath.AffordArithmeticSeries(cost - 1e-9, 10, 5, 0),
            Is.EqualTo(new BigDouble(4))
        );
    }

    [Test]
    public void PurchaseCount_ExactlyRepresentableGeometricPrice()
    {
        BigDouble budget = 317440;
        var below = new BigDouble(Math.BitDecrement(317440.0));
        Assert.That(BigMath.AffordGeometricSeries(budget, 10, 2, 10), Is.EqualTo(new BigDouble(5)));
        Assert.That(BigMath.AffordGeometricSeries(below, 10, 2, 10), Is.EqualTo(new BigDouble(4)));
        Assert.That(BigMath.SumGeometricSeries(5, 10, 2, 10), Is.EqualTo(budget));
    }
}
