using System.Diagnostics;
using LD.GameSample;

var state = new GameState();
var renderer = new Renderer();
var buyMode = BuyMode.One;
var clock = Stopwatch.StartNew();

renderer.Draw(state, buyMode);
while (true)
{
    var command = ReadCommand();
    state.Tick(clock.Elapsed.TotalSeconds);
    clock.Restart();

    switch (command)
    {
        case -1 or 'q' or 'Q':
            return;
        case ' ':
            state.Click();
            break;
        case int digit when digit >= '1' && digit < '1' + state.Generators.Count:
            state.TryBuy(state.Generators[digit - '1'], buyMode);
            break;
        case 'm' or 'M':
            buyMode = buyMode == BuyMode.Max ? BuyMode.One : buyMode + 1;
            break;
        case null when !Console.IsOutputRedirected && state.GoldPerSecond > 0:
            break;
        default:
            continue;
    }

    renderer.Draw(state, buyMode);
}

static int? ReadCommand()
{
    if (Console.IsInputRedirected)
    {
        return Console.Read();
    }
    if (Console.KeyAvailable)
    {
        return Console.ReadKey(intercept: true).KeyChar;
    }

    Thread.Sleep(100);
    return null;
}
