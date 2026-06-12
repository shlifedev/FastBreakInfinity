using LD.Numeric.IdleNumber;

namespace LD.Numeric.Test;

/// <summary>
/// 골드를 복잡한 연산 체인(곱셈/덧셈/뺄셈/나눗셈)으로 굴려 거대한 값을 만든 뒤,
/// 보유 골드와 거의 일치하는 가격의 아이템을 구매할 때 판정이 어긋나지 않는지 검증.
/// "분명 돈이 더 많은데 못 사는" 류의 비교/연산 불일치를 잡기 위한 테스트.
/// </summary>
public class ComplexGameScenarioTests
{
    /// <summary>
    /// 비교 연산자 삼분법 검증: 두 값에 대해 &lt;, ==, &gt; 중 정확히 하나만 참이어야 하고
    /// &gt;=, &lt;=, CompareTo가 모두 그 결과와 일치해야 함.
    /// </summary>
    private static void AssertOrderingConsistent(BigDouble a, BigDouble b)
    {
        bool lt = a < b;
        bool gt = a > b;
        bool eq = a == b;

        int trueCount = (lt ? 1 : 0) + (gt ? 1 : 0) + (eq ? 1 : 0);
        Assert.That(trueCount, Is.EqualTo(1), $"삼분법 위반: lt={lt}, gt={gt}, eq={eq}");

        Assert.That(a >= b, Is.EqualTo(gt || eq), ">= 는 (> 또는 ==)와 일치해야 함");
        Assert.That(a <= b, Is.EqualTo(lt || eq), "<= 는 (< 또는 ==)와 일치해야 함");

        int expectedSign =
            lt ? -1
            : gt ? 1
            : 0;
        Assert.That(Math.Sign(a.CompareTo(b)), Is.EqualTo(expectedSign), "CompareTo 부호 불일치");
    }

    /// <summary>
    /// money >= price 라면 반드시 구매가 가능해야 하고, 잔액이 음수가 되면 안 됨.
    /// </summary>
    private static void AssertPurchasable(BigDouble money, BigDouble price)
    {
        Assert.That(money >= price, Is.True, "money >= price 인데 구매 판정 실패");

        BigDouble remaining = money - price;
        Assert.That(
            remaining >= BigDouble.Zero,
            Is.True,
            $"구매 후 잔액이 음수: {remaining.ToStringMantissaExponent()}"
        );
        Assert.That(remaining <= money, Is.True, "잔액이 원금보다 클 수 없음");
    }

    [Test]
    public void Purchase_ExactPrice_AfterLongOperationChain()
    {
        // 동일한 연산 체인으로 돈과 가격을 각각 계산 → 정확히 같아야 하고 전액 구매 가능
        static BigDouble BuildValue()
        {
            BigDouble v = new BigDouble(1234.5678);
            v *= new BigDouble("7.77e77");
            v += new BigDouble("3.33e70");
            v -= new BigDouble("1.11e65");
            v /= 13;
            v *= new BigDouble("9.99e999");
            v += v / 3;
            return v;
        }

        BigDouble money = BuildValue();
        BigDouble price = BuildValue();

        Assert.That(money == price, Is.True, "같은 체인으로 계산한 값은 같아야 함");
        AssertPurchasable(money, price);

        BigDouble remaining = money - price;
        Assert.That(remaining == BigDouble.Zero, Is.True, "전액 구매 후 잔액은 0");
    }

    [Test]
    public void Purchase_CommutedComputation_SamePrice()
    {
        // 곱셈 교환은 IEEE에서도 비트 단위로 동일 → 다른 순서로 계산해도 같아야 함
        BigDouble a = new BigDouble("1.7320508e500");
        BigDouble b = new BigDouble("2.7182818e300");
        BigDouble c = new BigDouble("3.1415926e799");

        BigDouble money = a * b + c;
        BigDouble price = c + b * a;

        Assert.That(money == price, Is.True, "교환법칙이 성립하는 식은 같은 값");
        AssertPurchasable(money, price);
        Assert.That(money - price == BigDouble.Zero, Is.True);
    }

    [Test]
    public void Purchase_AssociativityDrift_JudgmentStaysConsistent()
    {
        // (a+b)+c 와 a+(b+c)는 부동소수점 특성상 미세하게 다를 수 있음.
        // 핵심은 "다르더라도 비교 판정이 모순되지 않는 것" — 작은 쪽 가격은 큰 쪽 돈으로 반드시 구매 가능.
        BigDouble a = new BigDouble("1.111111111111111e1000");
        BigDouble b = new BigDouble("7.777777777777777e999");
        BigDouble c = new BigDouble("3.333333333333333e998");

        BigDouble money = a + b + c;
        BigDouble price = a + (b + c);

        AssertOrderingConsistent(money, price);
        Assert.That(money.Equals(price, 1e-9), Is.True, "결합 순서 차이는 1e-9 이내");

        BigDouble bigger = BigDouble.Max(money, price);
        BigDouble smaller = BigDouble.Min(money, price);
        AssertPurchasable(bigger, smaller);
    }

    [Test]
    public void Purchase_SlightlyCheaperItem_AlwaysBuyable()
    {
        // 가격이 돈보다 아주 미세하게(상대 1e-12) 싸면 반드시 구매 가능해야 함
        BigDouble money = new BigDouble("5.5555e5555");
        BigDouble price = money * new BigDouble(1.0 - 1e-12);

        Assert.That(price < money, Is.True, "가격이 미세하게 싸야 함");
        AssertPurchasable(money, price);

        BigDouble remaining = money - price;
        Assert.That(remaining > BigDouble.Zero, Is.True, "잔액은 0보다 커야 함");
        Assert.That(
            remaining.Equals(money * new BigDouble(1e-12), 1e-3),
            Is.True,
            $"잔액은 대략 money*1e-12: {remaining.ToStringMantissaExponent()}"
        );
    }

    [Test]
    public void Purchase_SlightlyMoreExpensiveItem_NeverBuyable()
    {
        // 가격이 돈보다 미세하게(상대 1e-12) 비싸면 절대 구매 불가
        BigDouble money = new BigDouble("5.5555e5555");
        BigDouble price = money * new BigDouble(1.0 + 1e-12);

        Assert.That(money < price, Is.True, "돈이 미세하게 부족");
        Assert.That(money >= price, Is.False, "구매 가능으로 잘못 판정되면 안 됨");
        AssertOrderingConsistent(money, price);
    }

    [Test]
    public void Purchase_FullSpend_RepeatedEarnAndDrain()
    {
        // 매번 다른 연산 체인으로 돈을 벌고, 전액과 일치하는 가격으로 모두 소진 — 항상 0으로 떨어져야 함
        BigDouble money = BigDouble.Zero;

        for (int i = 1; i <= 30; i++)
        {
            BigDouble earned = new BigDouble(1.0 + i * 0.137, i * 100);
            earned *= new BigDouble(2.5, i * 7);
            earned += new BigDouble(9.9, i * 50);
            earned /= 3;

            money += earned;
            BigDouble price = money; // 보유 전액과 동일한 가격

            AssertPurchasable(money, price);
            money -= price;

            Assert.That(money == BigDouble.Zero, Is.True, $"{i}회차 전액 구매 후 잔액은 0");
        }
    }

    [Test]
    public void Purchase_HalfDrain_NeverGoesNegative()
    {
        // 성장(곱셈+수입)과 절반 지출을 반복 — 잔액이 음수가 되거나 판정이 어긋나면 안 됨
        BigDouble money = new BigDouble("1e10");
        BigDouble income = new BigDouble("3.7e9");

        for (int i = 0; i < 200; i++)
        {
            money = money * new BigDouble(1.07) + income;

            BigDouble price = money / 2;
            Assert.That(money >= price, Is.True, $"{i}회차: 절반 가격은 항상 구매 가능");

            money -= price;
            Assert.That(money >= BigDouble.Zero, Is.True, $"{i}회차: 잔액 음수");
            AssertOrderingConsistent(money, price);
        }
    }

    [Test]
    public void Purchase_AccumulatedIncome_VsBulkPrice()
    {
        // 수입을 1000번 누적한 돈 vs (수입 * 1000)으로 책정된 가격.
        // 부동소수점 누적 오차로 미세하게 다를 수 있지만, 판정은 모순 없어야 하고 차이는 극미해야 함
        BigDouble income = new BigDouble(1.23456789, 45);
        BigDouble money = BigDouble.Zero;

        for (int i = 0; i < 1000; i++)
        {
            money += income;
        }

        BigDouble price = income * 1000;

        Assert.That(money.Equals(price, 1e-9), Is.True, "누적합과 일괄 곱은 1e-9 이내로 일치");
        AssertOrderingConsistent(money, price);

        BigDouble bigger = BigDouble.Max(money, price);
        BigDouble smaller = BigDouble.Min(money, price);
        AssertPurchasable(bigger, smaller);
    }

    [Test]
    public void Purchase_DivideMultiplyRoundTrip()
    {
        // 가격을 money/7*7 로 책정 — 왕복 연산 오차가 있어도 판정은 일관돼야 함
        BigDouble money = new BigDouble("8.8888888888888e8888");
        BigDouble price = money / 7 * 7;

        Assert.That(money.Equals(price, 1e-12), Is.True, "나눗셈 왕복 오차는 1e-12 이내");
        AssertOrderingConsistent(money, price);

        if (money >= price)
        {
            AssertPurchasable(money, price);
        }
        else
        {
            // 왕복 오차로 가격이 미세하게 비싸진 경우 — 부족분은 상대 1e-12 미만이어야 함
            BigDouble shortfall = price - money;
            Assert.That(
                shortfall < money * new BigDouble(1e-12),
                Is.True,
                $"부족분이 비정상적으로 큼: {shortfall.ToStringMantissaExponent()}"
            );
        }
    }

    [Test]
    public void Purchase_NearEqualAtHugeExponent()
    {
        // 거대 지수(e100000)에서 마지막 유효숫자 한 끗 차이의 구매 판정
        BigDouble money = new BigDouble("1.000000000000001e100000");
        BigDouble priceCheaper = new BigDouble("1.000000000000000e100000");
        BigDouble priceExpensive = new BigDouble("1.000000000000002e100000");

        AssertPurchasable(money, priceCheaper);
        Assert.That(money >= priceExpensive, Is.False, "더 비싼 아이템은 구매 불가");

        AssertOrderingConsistent(money, priceCheaper);
        AssertOrderingConsistent(money, priceExpensive);
        AssertOrderingConsistent(money, money);
    }
}
