using LD.Numeric.IdleNumber;

namespace LD.Numeric.Test;

/// <summary>
/// BigMath 테스트 — 기하/산술 시리즈 비용 계산과 구매 효율
/// </summary>
public class BigMathTests
{
    // ===== AffordGeometricSeries =====

    [Test]
    public void AffordGeometricSeries_Basic()
    {
        // 가격 1부터 2배씩: 1+2+4+8+16+32 = 63 ≤ 100 < 127 → 6개 구매 가능
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
        // 이미 2개 보유 → 실제 시작가 4: 4+8+16+32 = 60 ≤ 100 < 124 → 4개
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
        // 1e100 재화, 시작가 1e10, 비율 10 → n = floor(log10(1e90 * 9 + 1)) ≈ 90
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

    // ===== SumGeometricSeries =====

    [Test]
    public void SumGeometricSeries_Basic()
    {
        // 10 + 20 + 40 = 70
        var cost = BigMath.SumGeometricSeries(3, 10, 2, 0);
        Assert.That(cost.Equals(new BigDouble(70), 1e-9), Is.True, $"기대 70, 실제 {cost}");
    }

    [Test]
    public void SumGeometricSeries_WithCurrentOwned()
    {
        // 1개 보유, 시작가 10, 비율 2 → 실제 시작가 20: 20 + 40 = 60
        var cost = BigMath.SumGeometricSeries(2, 10, 2, 1);
        Assert.That(cost.Equals(new BigDouble(60), 1e-9), Is.True, $"기대 60, 실제 {cost}");
    }

    [Test]
    public void SumGeometricSeries_RatioOne_IsLinearCost()
    {
        // 비율 1이면 가격이 고정 — 10짜리 5개 = 50 (1-ratio 0 나눗셈으로 NaN이 되면 안 됨)
        var cost = BigMath.SumGeometricSeries(5, 10, 1, 0);
        Assert.That(cost.Equals(new BigDouble(50), 1e-9), Is.True, $"기대 50, 실제 {cost}");
    }

    [Test]
    public void AffordGeometricSeries_RatioOne_IsLinearAfford()
    {
        // 비율 1이면 고정가 10짜리를 100으로 정확히 10개
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
        // Sum으로 계산한 비용만큼 주면 정확히 그 개수를 살 수 있어야 함
        BigDouble priceStart = 5;
        BigDouble ratio = 1.5;
        BigDouble owned = 3;
        BigDouble count = 7;

        // 정확한 경계값은 log/floor 오차로 한 개 아래로 떨어질 수 있어 약간의 버퍼를 줌
        var cost = BigMath.SumGeometricSeries(count, priceStart, ratio, owned) * 1.000001;
        var affordable = BigMath.AffordGeometricSeries(cost, priceStart, ratio, owned);
        Assert.That(affordable.Equals(count, 1e-6), Is.True, $"기대 {count}, 실제 {affordable}");
    }

    // ===== AffordArithmeticSeries =====

    [Test]
    public void AffordArithmeticSeries_Basic()
    {
        // 10+15+20+25+30 = 100 → 정확히 5개
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
        // 2개 보유 → 시작가 20: 20+25 = 45 ≤ 50 < 75 → 2개
        var affordable = BigMath.AffordArithmeticSeries(50, 10, 5, 2);
        Assert.That(
            affordable.Equals(new BigDouble(2), 1e-9),
            Is.True,
            $"기대 2, 실제 {affordable}"
        );
    }

    // ===== SumArithmeticSeries =====

    [Test]
    public void SumArithmeticSeries_Basic()
    {
        // 10+15+20+25 = 70
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

        // 정확한 경계값은 sqrt/floor 오차로 한 개 아래로 떨어질 수 있어 약간의 버퍼를 줌
        var cost = BigMath.SumArithmeticSeries(count, priceStart, priceAdd, owned) * 1.000001;
        var affordable = BigMath.AffordArithmeticSeries(cost, priceStart, priceAdd, owned);
        Assert.That(affordable.Equals(count, 1e-6), Is.True, $"기대 {count}, 실제 {affordable}");
    }

    // ===== EfficiencyOfPurchase =====

    [Test]
    public void EfficiencyOfPurchase_Basic()
    {
        // 10/100 + 10/1 = 10.1
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
        // 같은 비용이면 RpS 증가가 큰 쪽이 효율 점수가 낮아야 함
        var better = BigMath.EfficiencyOfPurchase(100, 50, 20);
        var worse = BigMath.EfficiencyOfPurchase(100, 50, 5);
        Assert.That(better < worse, Is.True);
    }

    // ===== RandomBigDouble =====

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
}
