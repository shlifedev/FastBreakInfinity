using LD.Numeric.IdleNumber;

namespace LD.GameSample;

/// <summary>
/// 골드를 자동 생산하는 시설. 구매할수록 가격이 기하급수적으로 오른다.
/// </summary>
public class Generator
{
    public string Name { get; }
    public BigDouble BaseCost { get; }
    public BigDouble CostRatio { get; }
    public BigDouble BaseProduction { get; }
    public BigDouble Owned { get; set; } = BigDouble.Zero;

    public Generator(string name, BigDouble baseCost, double costRatio, BigDouble baseProduction)
    {
        Name = name;
        BaseCost = baseCost;
        CostRatio = costRatio;
        BaseProduction = baseProduction;
    }

    public BigDouble ProductionPerSecond => BaseProduction * Owned;

    /// <summary>
    /// 다음 count개를 구매하는 데 드는 총 가격 (기하급수 합)
    /// </summary>
    public BigDouble CostOf(BigDouble count)
    {
        return BigMath.SumGeometricSeries(count, BaseCost, CostRatio, Owned);
    }

    /// <summary>
    /// 주어진 골드로 살 수 있는 최대 수량
    /// </summary>
    public BigDouble MaxAffordable(BigDouble gold)
    {
        return BigMath.AffordGeometricSeries(gold, BaseCost, CostRatio, Owned);
    }
}
