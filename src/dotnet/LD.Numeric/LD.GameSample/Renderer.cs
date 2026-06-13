using System.Text;
using LD.Numeric.IdleNumber;

namespace LD.GameSample;

/// <summary>
/// 콘솔 화면 그리기. 커서를 맨 위로 옮기고 덮어써서 깜빡임을 줄인다.
/// </summary>
public class Renderer
{
    // 한글은 콘솔에서 2칸을 차지해 공백 패딩으론 이전 프레임 잔상을 정확히 못 지운다.
    // ANSI escape로 줄 끝(\e[K)과 화면 아래(\e[J)를 지우는 방식을 쓴다.
    private const string ClearLineEnd = "\x1b[K";
    private const string ClearBelow = "\x1b[J";

    private readonly bool _redirected = Console.IsOutputRedirected;
    private readonly StringBuilder _buffer = new();

    public void Draw(GameState state, BuyMode buyMode)
    {
        _buffer.Clear();

        AppendLine("=== LD.Numeric 아이들 게임 샘플 ===");
        AppendLine("");
        // ToString()은 알파벳 단위(1.50A), ToStringMantissaExponent()는 내부 표현을 보여준다
        AppendLine($"골드     : {state.Gold}  ({state.Gold.ToStringMantissaExponent()})");
        AppendLine($"초당 생산: {state.GoldPerSecond}/s");
        AppendLine($"구매 모드: {BuyModeLabel(buyMode)}");
        AppendLine("");

        for (int i = 0; i < state.Generators.Count; i++)
        {
            var generator = state.Generators[i];
            var count = state.BuyCountOf(generator, buyMode);
            string priceText;
            if (count <= BigDouble.Zero)
            {
                priceText = $"가격 {generator.CostOf(BigDouble.One)} (구매 불가)";
            }
            else
            {
                var cost = generator.CostOf(count);
                var affordable = cost <= state.Gold ? "구매 가능" : "골드 부족";
                priceText = $"x{count} 가격 {cost} ({affordable})";
            }

            AppendLine(
                $"[{i + 1}] {generator.Name, -6} 보유 {generator.Owned, -8} "
                    + $"생산 {generator.ProductionPerSecond}/s  {priceText}"
            );
        }

        AppendLine("");
        AppendLine("[Space] 클릭  [1~4] 구매  [M] 구매 모드 전환  [Q] 종료");

        if (_redirected)
        {
            Console.WriteLine(_buffer.ToString());
        }
        else
        {
            Console.SetCursorPosition(0, 0);
            _buffer.Append(ClearBelow);
            Console.Write(_buffer.ToString());
        }
    }

    public static string BuyModeLabel(BuyMode mode)
    {
        return mode switch
        {
            BuyMode.One => "x1",
            BuyMode.Ten => "x10",
            BuyMode.Hundred => "x100",
            BuyMode.Max => "MAX",
            _ => "?",
        };
    }

    private void AppendLine(string text)
    {
        _buffer.Append(text);
        if (!_redirected)
        {
            _buffer.Append(ClearLineEnd);
        }
        _buffer.AppendLine();
    }
}
