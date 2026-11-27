using LD.Numeric.IdleNumber;

namespace LD.Numeric.Test;

public class GameScenarioTests
{
    [Test]
    public void GameScenario_CurrencyAccumulation()
    {
        BigDouble currency = BigDouble.Zero;
        BigDouble income = new BigDouble(1.5, 10);

        for (int i = 0; i < 1000; i++)
        {
            currency += income;
        }

        BigDouble expected = new BigDouble(1.5, 13);

        Assert.That(
            currency.Equals(expected, 1e-10),
            Is.True,
            $"누적 결과: {currency.Mantissa}e{currency.Exponent}"
        );
    }

    [Test]
    public void GameScenario_Purchase()
    {
        BigDouble money = new BigDouble("1.5e100");
        BigDouble price = new BigDouble("1.2e100");

        Assert.That(money >= price, Is.True, "구매 가능 확인");

        money -= price;

        Assert.That(money.Exponent == 99, Is.True);
        Assert.That(Math.Abs(money.Mantissa - 3.0) < 0.1, Is.True);
    }

    [Test]
    public void GameScenario_Multiplier()
    {
        BigDouble baseValue = new BigDouble("1e50");
        BigDouble multiplier = new BigDouble(2.5);

        BigDouble result = baseValue * multiplier;

        Assert.That(result == new BigDouble(2.5, 50), Is.True);
    }

    [Test]
    public void GameScenario_ExponentialGrowth()
    {
        BigDouble value = new BigDouble(1.0);
        BigDouble growthRate = new BigDouble(1.1);

        for (int i = 0; i < 100; i++)
        {
            value *= growthRate;
        }

        Assert.That(value.Exponent >= 3 && value.Exponent <= 5, Is.True, "1.1^100의 지수는 4 근처");
    }
}
