using LD.Numeric.IdleNumber;

namespace LD.GameSample;

public enum BuyMode
{
    One,
    Ten,
    Hundred,
    Max,
}

/// <summary>
/// 게임 로직 전체. Console에 의존하지 않아 시뮬레이션/테스트로 돌릴 수 있다.
/// </summary>
public class GameState
{
    public BigDouble Gold { get; private set; } = BigDouble.Zero;
    public BigDouble GoldPerClick { get; } = BigDouble.One;
    public IReadOnlyList<Generator> Generators { get; }

    public GameState()
    {
        // 티어 간 비용 격차를 크게 잡아 플레이 몇 분 안에 알파벳 단위(A, B...)가 등장한다
        Generators = new List<Generator>
        {
            new("광부", baseCost: 10, costRatio: 1.15, baseProduction: 1),
            new("광산", baseCost: 500, costRatio: 1.15, baseProduction: 40),
            new("공장", baseCost: "2.5e4", costRatio: 1.15, baseProduction: 2_000),
            new("행성 코어", baseCost: "1e12", costRatio: 1.20, baseProduction: "5e8"),
        };
    }

    public BigDouble GoldPerSecond
    {
        get
        {
            var total = BigDouble.Zero;
            foreach (var generator in Generators)
            {
                total += generator.ProductionPerSecond;
            }
            return total;
        }
    }

    public void Tick(double deltaSeconds)
    {
        if (deltaSeconds <= 0)
        {
            return;
        }
        Gold += GoldPerSecond * deltaSeconds;
    }

    public void Click()
    {
        Gold += GoldPerClick;
    }

    public BigDouble BuyCountOf(Generator generator, BuyMode mode)
    {
        return mode switch
        {
            BuyMode.One => BigDouble.One,
            BuyMode.Ten => 10,
            BuyMode.Hundred => 100,
            BuyMode.Max => generator.MaxAffordable(Gold),
            _ => BigDouble.Zero,
        };
    }

    public bool TryBuy(Generator generator, BuyMode mode)
    {
        var count = BuyCountOf(generator, mode);
        if (count <= BigDouble.Zero)
        {
            return false;
        }

        var cost = generator.CostOf(count);
        if (cost > Gold)
        {
            return false;
        }

        Gold -= cost;
        generator.Owned += count;
        return true;
    }
}
