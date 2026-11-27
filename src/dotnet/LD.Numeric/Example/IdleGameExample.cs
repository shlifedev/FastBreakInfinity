using System;
using LD.Numeric.IdleNumber;

namespace LD.Numeric.Example;

public class IdleGameExample
{
    private BigDouble _gold;
    private BigDouble _goldPerClick;
    private BigDouble _goldPerSecond;

    private BigDouble _clickUpgradeCost;
    private BigDouble _autoUpgradeCost;

    private int _clickLevel;
    private int _autoLevel;

    public IdleGameExample()
    {
        _gold = BigDouble.Zero;
        _goldPerClick = BigDouble.One;
        _goldPerSecond = BigDouble.Zero;

        _clickUpgradeCost = new BigDouble(10);
        _autoUpgradeCost = new BigDouble(50);

        _clickLevel = 1;
        _autoLevel = 0;
    }

    public void Click()
    {
        _gold += _goldPerClick;
        Console.WriteLine($"[클릭] +{_goldPerClick} 골드 획득!");
    }

    public void AutoProduce()
    {
        if (_goldPerSecond > BigDouble.Zero)
        {
            _gold += _goldPerSecond;
            Console.WriteLine($"[자동] +{_goldPerSecond} 골드 생산");
        }
    }

    public bool BuyClickUpgrade()
    {
        if (_gold >= _clickUpgradeCost)
        {
            _gold -= _clickUpgradeCost;
            _clickLevel++;

            _goldPerClick *= 2;

            _clickUpgradeCost *= new BigDouble(1.5);

            Console.WriteLine($"[업그레이드] 클릭 Lv.{_clickLevel}! 클릭당 {_goldPerClick} 골드");
            return true;
        }

        Console.WriteLine($"[실패] 골드 부족! 필요: {_clickUpgradeCost}");
        return false;
    }

    public bool BuyAutoUpgrade()
    {
        if (_gold >= _autoUpgradeCost)
        {
            _gold -= _autoUpgradeCost;
            _autoLevel++;

            if (_autoLevel == 1)
            {
                _goldPerSecond = BigDouble.One;
            }
            else
            {
                _goldPerSecond *= new BigDouble(1.5);
            }

            _autoUpgradeCost *= 2;

            Console.WriteLine($"[업그레이드] 자동생산 Lv.{_autoLevel}! 초당 {_goldPerSecond} 골드");
            return true;
        }

        Console.WriteLine($"[실패] 골드 부족! 필요: {_autoUpgradeCost}");
        return false;
    }

    public void ShowStatus()
    {
        Console.WriteLine("\n========== 게임 상태 ==========");
        Console.WriteLine($"  보유 골드: {_gold}");
        Console.WriteLine($"  클릭당 골드: {_goldPerClick} (Lv.{_clickLevel})");
        Console.WriteLine($"  초당 골드: {_goldPerSecond} (Lv.{_autoLevel})");
        Console.WriteLine("--------------------------------");
        Console.WriteLine($"  [1] 클릭 업그레이드 비용: {_clickUpgradeCost}");
        Console.WriteLine($"  [2] 자동생산 업그레이드 비용: {_autoUpgradeCost}");
        Console.WriteLine("================================\n");
    }

    public void ShowMenu()
    {
        Console.WriteLine(
            "명령어: [c]클릭 [1]클릭업그레이드 [2]자동업그레이드 [s]상태 [a]자동생산 [q]종료"
        );
    }

    public void Run()
    {
        Console.WriteLine("=== BigDouble 아이들 게임 ===");
        Console.WriteLine("큰 숫자를 다루는 아이들 게임 예시입니다.\n");

        ShowStatus();
        ShowMenu();

        while (true)
        {
            Console.Write("> ");
            var input = Console.ReadLine()?.ToLower();

            switch (input)
            {
                case "c":
                    Click();
                    break;
                case "1":
                    BuyClickUpgrade();
                    break;
                case "2":
                    BuyAutoUpgrade();
                    break;
                case "s":
                    ShowStatus();
                    break;
                case "a":
                    AutoProduce();
                    break;
                case null:
                case "q":
                    Console.WriteLine("게임 종료!");
                    return;
                case "cheat":
                    _gold += new BigDouble("1e100");
                    Console.WriteLine("[치트] 1e100 골드 추가!");
                    break;
                default:
                    ShowMenu();
                    break;
            }
        }
    }
}

public class BigDoubleDemo
{
    public static void RunDemo()
    {
        Console.WriteLine("=== BigDouble 기능 데모 ===\n");

        Console.WriteLine("1. 생성자 예시:");
        var a = new BigDouble(1.5, 100);
        var b = new BigDouble("1.23e50");
        var c = new BigDouble(12345.6789);
        Console.WriteLine($"   new BigDouble(1.5, 100) = {a.ToStringMantissaExponent()}");
        Console.WriteLine($"   new BigDouble(\"1.23e50\") = {b.ToStringMantissaExponent()}");
        Console.WriteLine($"   new BigDouble(12345.6789) = {c}");

        Console.WriteLine("\n2. 사칙연산:");
        var x = new BigDouble("1e100");
        var y = new BigDouble("2e100");
        Console.WriteLine($"   {x} + {y} = {x + y}");
        Console.WriteLine($"   {y} - {x} = {y - x}");
        Console.WriteLine($"   {x} * {y} = {x * y}");
        Console.WriteLine($"   {y} / {x} = {y / x}");

        Console.WriteLine("\n3. 비교 연산:");
        var small = new BigDouble("1e50");
        var large = new BigDouble("1e100");
        Console.WriteLine($"   {small} < {large} = {small < large}");
        Console.WriteLine(
            $"   {small} == {new BigDouble("1e50")} = {small == new BigDouble("1e50")}"
        );
        Console.WriteLine($"   {large} > {small} = {large > small}");

        Console.WriteLine("\n4. 수학 함수:");
        var val = new BigDouble("1e10");
        Console.WriteLine($"   Pow({val}, 3) = {BigDouble.Pow(val, 3)}");
        Console.WriteLine($"   Sqrt({val}) = {BigDouble.Sqrt(val)}");
        Console.WriteLine($"   Log10({val}) = {BigDouble.Log10(val)}");

        Console.WriteLine("\n5. 알파벳 단위 표시:");
        var values = new[]
        {
            new BigDouble(1.234, 3),
            new BigDouble(1.234, 6),
            new BigDouble(1.234, 78),
            new BigDouble(1.234, 81),
            new BigDouble(1.234, 84),
        };
        foreach (var v in values)
        {
            Console.WriteLine($"   {v.ToStringMantissaExponent()} = {v}");
        }

        Console.WriteLine("\n6. 매우 큰 숫자 처리:");
        var huge1 = new BigDouble("1e999999");
        var huge2 = new BigDouble("2e999999");
        Console.WriteLine($"   1e999999 + 2e999999 = {huge1 + huge2}");
        Console.WriteLine($"   결과 지수: {(huge1 + huge2).Exponent}");

        Console.WriteLine("\n7. 게임 시나리오 - 100번 1.1배 성장:");
        var growth = BigDouble.One;
        for (int i = 0; i < 100; i++)
        {
            growth *= new BigDouble(1.1);
        }
        Console.WriteLine($"   1.0 * 1.1^100 = {growth}");

        Console.WriteLine("\n=== 데모 종료 ===");
    }
}

public static class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("BigDouble 예시 프로그램");
        Console.WriteLine("1. 아이들 게임 예시");
        Console.WriteLine("2. BigDouble 기능 데모");
        Console.Write("선택: ");

        var choice = Console.ReadLine();
        Console.WriteLine();

        switch (choice)
        {
            case null:
                return;
            case "1":
                var game = new IdleGameExample();
                game.Run();
                break;
            case "2":
                BigDoubleDemo.RunDemo();
                break;
            default:
                Console.WriteLine("잘못된 선택입니다.");
                break;
        }
    }
}
