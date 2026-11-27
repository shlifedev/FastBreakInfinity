using LD.Numeric.IdleNumber;

namespace LD.GameSample;

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

    public BigDouble CostOf(BigDouble count)
    {
        return BigMath.SumGeometricSeries(count, BaseCost, CostRatio, Owned);
    }

    public BigDouble MaxAffordable(BigDouble gold)
    {
        return BigMath.AffordGeometricSeries(gold, BaseCost, CostRatio, Owned);
    }
}
