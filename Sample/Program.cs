using PokemonBDSPRNGLibrary.RestoreSeed;
using PokemonBDSPRNGLibrary.StarterRNG;
using PokemonPRNG.XorShift128;
using static System.Console;

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

        var adv = 82782;
        var rand = seed.Next((uint)adv);

        var prev = -1;
        var count = 0;

        var pktimer = RandomRange(0.0, 12.0);
        var t = 0.0;
        for (int i = 0; count < 8; i++)
        {
            t += 61.0 / 60.0;
            if (pktimer < t)
            {
                var pkInterval = rand.BlinkPokemon();
                adv++;
                pktimer += pkInterval;
                Write("P ");
            }

            // 観測値を取得
            var blink = rand.BlinkPlayer();
            adv++;
            if (blink != PlayerBlink.None)
            {
                if (prev != -1)
                {
                    var interval = i - prev;
                    searcher.AddInterval((uint)interval);
                    count++;
                }
                prev = i;
            }

            Write($"{blink.ToShortString()} ");
        }

        var (idx, rest, restored) = searcher.SearchInNoisy(seed, 100000u).FirstOrDefault();

        WriteLine();

        WriteLine($"expected: {adv}");
        WriteLine($"index: {idx}");
        WriteLine(adv == idx && restored == rand ? "Successfully restored." : "Failed...");
        WriteLine(rest);
        ReadKey();
        WriteLine();
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
