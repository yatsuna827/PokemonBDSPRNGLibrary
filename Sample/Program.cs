using PokemonBDSPRNGLibrary.RestoreSeed;
using PokemonBDSPRNGLibrary.StarterRNG;
using PokemonBDSPRNGLibrary.Generators;
using System.Linq;
using PokemonStandardLibrary.CommonExtension;
using PokemonPRNG.XorShift128;
using Sample;
using static System.Console;

{
    var seed = (0x0u, 0x1u, 0x2u, 0x3u);
    var gen = new StaticSymbolGenerator("ディアルガ", 47, 3);
    foreach(var (i, res) in seed.EnumerateSeed().EnumerateGeneration(gen).Take(100000).WithIndex().Where(_ => _.element.Shiny.IsShiny()))
    {
        WriteLine($"{i}[F] {string.Join("-", res.IVs)} {res.Shiny.ToSymbol()}");
    }
}

uint RandomUint()
{
    var random = new Random();
    return (uint)random.Next(0x10000) << 16 | (uint)random.Next(0x10000);
}

double RandomRange(double min, double max)
{
    var random = new Random();
    return (double)random.NextDouble() * (max - min) + min;
}

(uint, uint, uint, uint) GenerateSeed()
{
    return (RandomUint(), RandomUint(), RandomUint(), RandomUint());
}

const int MODE = 3; // 0: ゴンベのデモ, 1: 主人公瞬きのデモ, 2:主人公瞬きからの再特定のデモ  3:主人公瞬きからの再特定のデモ

if (MODE == 0)
{
    const float esp = 0;
    WriteLine($"esp = {esp}[sec]");

    while (true)
    {
        var seed = GenerateSeed();
        var inverter = new MunchlaxInverter(esp);

        (uint, uint, uint, uint) restored;

        var rand = seed;
        while (true)
        {
            // 観測値を取得
            // 0.1以内でランダムに誤差を挿入する
            var interval = rand.BlinkMunchlax().Randomize();
            inverter.AddInterval(interval);

            if (inverter.TryRestoreState(out restored))
                break;
        }

        WriteLine();

        WriteLine($"expected: {seed.ToHexString()}");
        WriteLine($"restored: {restored.ToHexString()}");
        WriteLine(restored == seed ? "Successfully restored." : "Failed...");
        WriteLine($"blink: {inverter.BlinkCount} times");
        ReadKey();
        WriteLine();
    }
}
else if(MODE == 1)
{
    while (true)
    {
        var seed = GenerateSeed();
        var inverter = new PlayerBlinkInverter();

        (uint, uint, uint, uint) restored;

        var rand = seed;
        while (true)
        {
            // 観測値を取得
            var blink = rand.BlinkPlayer();
            inverter.AddBlink(blink);

            Write(blink == PlayerBlink.None ? "- " : blink == PlayerBlink.Single ? "s " : "d ");

            if (inverter.TryRestoreState(out restored))
                break;
        }

        WriteLine();

        WriteLine($"expected: {seed.ToHexString()}");
        WriteLine($"restored: {restored.ToHexString()}");
        WriteLine(restored == seed ? "Successfully restored." : "Failed...");
        WriteLine($"blink roll: {inverter.BlinkCount} times");
        ReadKey();
        WriteLine();
    }
}
else if(MODE == 2)
{
    
    while (true)
    {
        var seed = GenerateSeed();
        var searcher = new PlayerLinearSearch();

        uint idx;
        (uint, uint, uint, uint) restored;

        var adv = 0u;
        var rand = seed.Next((uint)adv);

        adv += rand.GetNextPlayerBlink();
        while (true)
        {
            // 観測値を取得
            var interval = rand.GetNextPlayerBlink(out var blink);
            searcher.AddInterval(interval);
            adv += interval;

            Write($"{string.Join(" ", Enumerable.Repeat('-', (int)interval))} {blink.ToShortString()} ");

            if (searcher.BlinkCount > 8)
            {
                (idx, restored) = searcher.Search(seed, 100000u).FirstOrDefault();
                break;
            }
        }

        WriteLine();

        WriteLine($"expected: {adv}");
        WriteLine($"index: {idx}");
        WriteLine(adv == idx && rand == restored ? "Successfully restored." : "Failed...");

        ReadKey();
        WriteLine();
    }
}
else if(MODE == 3)
{
    while (true)
    {
        var seed = GenerateSeed();
        var searcher = new PlayerLinearSearch();

        var adv = 8270;
        var rand = seed.Next((uint)adv);

        var pktimer = RandomRange(3.0, 12.0);
        rand.GetNextPlayerBlink();

        for (int i = 0; i < 8; i++)
        {
            var interval = rand.GetNextPlayerBlink(pktimer, out pktimer);
            searcher.AddInterval(interval);
            WriteLine($"{interval}, {pktimer}");
        }

        //var t1 = ExecutionTimer.Measure(() => { for(int i=0; i<10; i++) searcher.SearchInNoisy(seed, 10000u).FirstOrDefault(); });

        var (idx, rem, restored) = searcher.SearchInNoisy(seed, 10000u).FirstOrDefault();

        WriteLine();

        //WriteLine($"expected: {pktimer}");
        //WriteLine($"index: {rem}");
        WriteLine(restored == rand ? "Successfully restored." : "Failed...");
        //WriteLine($"{t1} [ms]");
        ReadKey();
        WriteLine();

        WriteLine($"rand: {rand.ToHexString()} timer: {pktimer}");

        var c1 = 0;
        for (int i=0; i<200; i++)
        {
            rand.BlinkPlayer(ref pktimer, ref c1);
            //searcher.AddInterval(interval);
            WriteLine($"{i} {c1} {rand.ToHexString()} {pktimer}");
        }

        WriteLine();

        rand = restored;
        pktimer = rem;
        c1 = 0;
        WriteLine($"rand: {rand.ToHexString()} timer: {pktimer}");
        for (int i = 0; i < 200; i++)
        {
            rand.BlinkPlayer(ref pktimer, ref c1);
            //searcher.AddInterval(interval);
            WriteLine($"{i} {c1} {rand.ToHexString()} {pktimer}");
        }

        ReadKey();
    }
}

static class Ext
{
    public static float Randomize(this float value, float ep = 0.1f, float min = 3.0f + 0.285f, float max = 12.0f + 0.285f)
    {
        var random = new Random();
        value += (float)(random.NextDouble() * ep) * (random.Next() % 2 == 0 ? 1 : -1);

        if (value < min) value = min;
        if (value > max) value = max;

        return value;
    }

    public static string ToHexString(this (uint s1, uint s2, uint s3, uint s4) state)
        => $"(0x{state.s1:X8}, 0x{state.s2:X8}, 0x{state.s3:X8}, 0x{state.s4:X8})";
}
