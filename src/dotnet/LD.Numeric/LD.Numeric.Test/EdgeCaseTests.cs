using LD.Numeric.IdleNumber;

namespace LD.Numeric.Test;

/// <summary>
/// BigDouble 경계값 및 극단 케이스 테스트
/// </summary>
public class EdgeCaseTests
{
    // ===== AdjustedMantissa 테스트 =====

    [Test]
    public void AdjustedMantissa_SideEffect()
    {
        // AdjustedMantissa가 내부적으로 mantissa를 수정하는 문제 확인
        BigDouble original = new BigDouble(1.23456789, 100);
        double originalMantissa = original.Mantissa;

        double adjusted = original.AdjustedMantissa();

        Console.WriteLine($"Original mantissa before: {originalMantissa}");
        Console.WriteLine($"Original mantissa after: {original.Mantissa}");
        Console.WriteLine($"Adjusted mantissa: {adjusted}");

        // 주의: 현재 구현에서 mantissa가 Round(mantissa, 6)으로 수정됨
        // 이것이 의도된 동작인지 버그인지 확인 필요
    }

    [Test]
    public void AdjustedMantissa_ConsistentResults()
    {
        BigDouble bd = new BigDouble(1.234567, 5);

        double first = bd.AdjustedMantissa();
        double second = bd.AdjustedMantissa();

        Assert.That(first == second, Is.True, "연속 호출 시 일관된 결과");
    }

    // ===== 경계값 테스트 =====

    [Test]
    public void EdgeCase_MaxExponent()
    {
        BigDouble bd = new BigDouble(1.0, long.MaxValue - 1);

        Assert.That(bd.Exponent == long.MaxValue - 1, Is.True);
    }

    [Test]
    public void EdgeCase_MinExponent()
    {
        BigDouble bd = new BigDouble(1.0, long.MinValue + 1);

        Assert.That(!BigDouble.IsNaN(bd), Is.True);
    }

    [Test]
    public void EdgeCase_DoublePrecisionLimit()
    {
        BigDouble a = new BigDouble(1.0000000000000001, 100);
        BigDouble b = new BigDouble(1.0000000000000002, 100);

        Console.WriteLine($"a.Mantissa: {a.Mantissa:R}");
        Console.WriteLine($"b.Mantissa: {b.Mantissa:R}");

        // double 정밀도 한계로 같을 수 있음
        if (a.Mantissa != b.Mantissa)
        {
            Assert.That(a != b, Is.True);
        }
    }

    [Test]
    public void EdgeCase_Denormalized()
    {
        BigDouble bd = new BigDouble(double.Epsilon, 0);

        Console.WriteLine($"Epsilon: {bd.Mantissa}e{bd.Exponent}");
        Assert.That(!BigDouble.IsNaN(bd), Is.True);
        Assert.That(!BigDouble.IsInfinity(bd), Is.True);
    }
}
